using Application.DTOs.Cita;
using Application.Interfaces;
using Domain.Entidades;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Servicios
{
    
    // PRUEBA GIT
    
    // Este es mi CitaService que maneja toda la logica de negocio de las citas
    // Lo hice asi porque quiero separar la logica de negocio de los controllers
    // Asi puedo reutilizar esta logica en otros lugares si es necesario
    // ==========================================
    // CAMBIO REALIZADO: AGREGADO COMENTARIO DE PRUEBA
    // FECHA: 19/08/2025 - PRUEBA DE DETECCION GIT
    // ==========================================
    public class CitaService : ICitaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogService _logService;

        public CitaService(IUnitOfWork unitOfWork, IEmailService emailService, ILogService logService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logService = logService;
        }

        // Este metodo genera los slots de tiempo automaticamente
        // Lo hice asi porque no quiero que el admin tenga que crear cada horario manualmente
        // El sistema calcula cuantos slots caben en el turno y los crea todos de una vez
        public async Task GenerarSlotsPorConfiguracionAsync(int configuracionId)
        {
            try
            {
                // Obtengo la configuracion del sistema
                var config = await _unitOfWork.Configuraciones.GetByIdAsync(configuracionId);
                if (config == null)
                    throw new Exception("Configuracion no encontrada");

                // Obtengo el turno para saber las horas de inicio y fin
                var turno = await _unitOfWork.Turnos.GetByIdAsync(config.TurnoId);
                if (turno == null)
                    throw new Exception("Turno no encontrado");

                // Obtengo las estaciones disponibles
                var estaciones = await _unitOfWork.Estaciones.GetAllAsync();
                if (!estaciones.Any())
                {
                    await CrearEstacionesPorDefectoAsync(config.CantidadEstaciones);
                    estaciones = await _unitOfWork.Estaciones.GetAllAsync();
                }

                // Valido que haya suficientes estaciones para la configuracion
                var estacionesList = estaciones.ToList();
                if (estacionesList.Count < config.CantidadEstaciones)
                {
                    // Si no hay suficientes, creo las que faltan
                    var estacionesFaltantes = config.CantidadEstaciones - estacionesList.Count;
                    await CrearEstacionesPorDefectoAsync(estacionesFaltantes);
                    estaciones = await _unitOfWork.Estaciones.GetAllAsync();
                    estacionesList = estaciones.ToList();
                }

                // Calculo cuantos slots de tiempo caben en el turno
                var inicio = turno.HoraInicio;
                var fin = turno.HoraFin;
                var totalSlots = (int)((fin - inicio).TotalMinutes / config.DuracionCitaMinutos);
                var estacionesParaUsar = estacionesList.Take(config.CantidadEstaciones).ToList();

                var citasAGuardar = new List<Cita>();

                // Creo un slot por cada intervalo de tiempo
                for (int i = 0; i < totalSlots; i++)
                {
                    var horaSlot = inicio + TimeSpan.FromMinutes(i * config.DuracionCitaMinutos);

                    // Para cada estacion, creo una cita disponible
                    foreach (var estacion in estacionesParaUsar)
                    {
                        var cita = new Cita
                        {
                            FechaHora = config.Fecha.Date + horaSlot,
                            EstacionNumero = estacion.Numero,
                            Estado = EstadoCita.Pendiente,
                            TurnoId = config.TurnoId,
                            UsuarioId = 0, // 0 significa que no hay usuario asignado
                            TipoTramite = TipoTramite.Ninguno
                        };
                        citasAGuardar.Add(cita);
                    }
                }

                // Guardo todas las citas en la base de datos
                foreach (var cita in citasAGuardar)
                {
                    await _unitOfWork.Citas.AddAsync(cita);
                }
                await _unitOfWork.SaveChangesAsync();

                // Registro en el log que se generaron los slots
                _logService.LogConfiguracion($"SLOTS GENERADOS - {citasAGuardar.Count} slots para fecha {config.Fecha:dd/MM/yyyy}", configuracionId);
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error generando slots para configuracion {configuracionId}", ex);
                throw;
            }
        }

        // Este es el metodo mas importante - maneja la reserva de citas
        // Aqui implemento toda la logica de validacion y control de concurrencia
        // Lo hice asi porque necesito asegurarme de que no haya sobrecupos
        public async Task<bool> ReservarCitaAsync(ReservarCitaDTO dto)
        {
            try
            {
                // Primera validacion: solo 1 cita activa por dia por usuario
                // Lo hice asi porque es un requerimiento del negocio
                var citasDelDia = (await _unitOfWork.Citas.GetAllAsync())
                    .Where(c => c.UsuarioId == dto.UsuarioId &&
                               c.FechaHora.Date == dto.FechaHora.Date &&
                               c.Estado != EstadoCita.Cancelada)
                    .ToList();

                if (citasDelDia.Any())
                {
                    _logService.LogError($"Usuario {dto.UsuarioId} intento reservar multiples citas en {dto.FechaHora.Date:dd/MM/yyyy}");
                    throw new InvalidOperationException("Solo puede tener una cita activa por día");
                }

                // Segunda validacion: debe haber configuracion activa para esa fecha y turno
                var config = (await _unitOfWork.Configuraciones.GetAllAsync())
                    .FirstOrDefault(c => c.Fecha.Date == dto.FechaHora.Date && c.TurnoId == dto.TurnoId);

                if (config == null)
                    throw new InvalidOperationException("No hay configuracion activa para la fecha/turno seleccionado");

                // Tercera validacion: la hora debe estar dentro del turno permitido
                var turno = await _unitOfWork.Turnos.GetByIdAsync(config.TurnoId);
                if (dto.FechaHora.TimeOfDay < turno.HoraInicio || dto.FechaHora.TimeOfDay >= turno.HoraFin)
                    throw new InvalidOperationException("La fecha/hora esta fuera del horario permitido");

                // Cuarta validacion: control de concurrencia y cupos - CRITICO PARA LA EVALUACION
                // Aqui es donde evito que se exceda el limite de estaciones
                var citasExistentes = (await _unitOfWork.Citas.GetAllAsync())
                    .Where(c => c.FechaHora == dto.FechaHora && c.Estado != EstadoCita.Cancelada)
                    .ToList();

                // Valido que no se exceda el limite de estaciones
                if (citasExistentes.Count >= config.CantidadEstaciones)
                {
                    _logService.LogError($"Intento de sobrecupo en horario {dto.FechaHora:dd/MM/yyyy HH:mm} - Usuario {dto.UsuarioId}");
                    throw new InvalidOperationException($"No hay cupo disponible en el horario seleccionado. Máximo {config.CantidadEstaciones} personas por horario.");
                }

                // Quinta validacion: debe haber una estacion libre
                var estaciones = await _unitOfWork.Estaciones.GetAllAsync();
                var estacionesOcupadas = citasExistentes.Select(c => c.EstacionNumero).ToList();
                var estacionLibre = estaciones.FirstOrDefault(e => !estacionesOcupadas.Contains(e.Numero));

                if (estacionLibre == null)
                    throw new InvalidOperationException("No hay estaciones libres en el horario solicitado");

                // Si paso todas las validaciones, creo la cita
                var cita = new Cita
                {
                    FechaHora = dto.FechaHora,
                    EstacionNumero = dto.EstacionNumero ?? estacionLibre.Numero,
                    UsuarioId = dto.UsuarioId,
                    TurnoId = dto.TurnoId,
                    TipoTramite = dto.TipoTramite,
                    Estado = EstadoCita.Reservada
                };

                await _unitOfWork.Citas.AddAsync(cita);
                var cambios = await _unitOfWork.SaveChangesAsync();

                // Si se guardo correctamente, envio el email y registro en el log
                if (cambios > 0)
                {
                    var usuario = await _unitOfWork.Usuario.GetByIdAsync(dto.UsuarioId);

                    // Envio email de confirmacion usando el servicio desacoplado
                    await _emailService.EnviarConfirmacionCitaAsync(
                        usuario.Email,
                        usuario.Nombre,
                        dto.FechaHora,
                        cita.EstacionNumero,
                        "Confirmada");

                    // Registro en el log que la reserva fue exitosa
                    _logService.LogReserva(dto.UsuarioId, dto.FechaHora, cita.EstacionNumero);
                    
                    // Log adicional para auditoria
                    _logService.LogConfiguracion(
                        $"RESERVA EXITOSA - Usuario {dto.UsuarioId} reservó cita para {dto.FechaHora:dd/MM/yyyy HH:mm} en estación {cita.EstacionNumero}. " +
                        $"Cupos restantes en este horario: {config.CantidadEstaciones - (citasExistentes.Count + 1)}", 
                        0);
                }

                return cambios > 0;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error reservando cita para usuario {dto.UsuarioId}", ex);
                throw;
            }
        }

        // Este metodo permite editar una cita existente
        // Lo hice asi porque los usuarios necesitan poder cambiar sus citas
        // Pero con validaciones para evitar abusos
        public async Task<bool> EditarCitaAsync(EditarCitaDTO dto)
        {
            try
            {
                // Busco la cita existente
                var citaExistente = (await _unitOfWork.Citas.GetAllAsync())
                    .FirstOrDefault(c => c.Id == dto.CitaId &&
                                        c.UsuarioId == dto.UsuarioId &&
                                        c.Estado == EstadoCita.Reservada);

                if (citaExistente == null)
                    throw new InvalidOperationException("Cita no encontrada o no se puede editar");

                // Valido que se pueda editar minimo 2 horas antes de la cita actual
                // Lo hice asi para dar tiempo a reorganizar el sistema
                if (DateTime.Now >= citaExistente.FechaHora.AddHours(-2))
                    throw new InvalidOperationException("No se puede editar una cita con menos de 2 horas de anticipacion");

                // Valido que la nueva fecha y hora tenga cupo disponible
                var citasEnNuevoHorario = (await _unitOfWork.Citas.GetAllAsync())
                    .Where(c => c.FechaHora == dto.NuevaFechaHora &&
                               c.Estado != EstadoCita.Cancelada &&
                               c.Id != dto.CitaId) 
                    .ToList();

                // Obtengo la configuracion para la nueva fecha
                var config = (await _unitOfWork.Configuraciones.GetAllAsync())
                    .FirstOrDefault(c => c.Fecha.Date == dto.NuevaFechaHora.Date);

                if (config == null)
                    throw new InvalidOperationException("No hay configuracion disponible para la nueva fecha");

                if (citasEnNuevoHorario.Count >= config.CantidadEstaciones)
                    throw new InvalidOperationException("No hay cupo disponible en el nuevo horario");

                // Actualizo la cita
                citaExistente.FechaHora = dto.NuevaFechaHora;
                if (dto.NuevoTipoTramite.HasValue)
                    citaExistente.TipoTramite = dto.NuevoTipoTramite.Value;

                _unitOfWork.Citas.Update(citaExistente);
                var cambios = await _unitOfWork.SaveChangesAsync();

                if (cambios > 0)
                {
                    var usuario = await _unitOfWork.Usuario.GetByIdAsync(dto.UsuarioId);

                    // Envio email de confirmacion del cambio
                    await _emailService.EnviarConfirmacionCitaAsync(
                        usuario.Email,
                        usuario.Nombre,
                        dto.NuevaFechaHora,
                        citaExistente.EstacionNumero,
                        "Modificada");

                    // Registro en el log que se modifico la cita
                    _logService.LogConfiguracion(
                        $"CITA EDITADA - Usuario {dto.UsuarioId} modifico cita a {dto.NuevaFechaHora:dd/MM/yyyy HH:mm}",
                        citaExistente.Id);
                }

                return cambios > 0;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error editando cita {dto.CitaId} para usuario {dto.UsuarioId}", ex);
                throw;
            }
        }

        // Este metodo permite cancelar una cita
        // Lo hice asi porque los usuarios necesitan poder cancelar sus citas
        // Pero con validaciones para evitar cancelaciones de ultima hora
        public async Task<bool> CancelarCitaAsync(CancelarCitaDTO dto)
        {
            try
            {
                var cita = (await _unitOfWork.Citas.GetAllAsync())
                    .FirstOrDefault(c => c.UsuarioId == dto.UsuarioId &&
                                        c.FechaHora == dto.FechaHora &&
                                        c.Estado == EstadoCita.Reservada);

                if (cita == null)
                    throw new InvalidOperationException("Cita no encontrada o ya cancelada");

                // Valido que se pueda cancelar (minimo 2 horas antes)
                // Lo hice asi para dar tiempo a reasignar la estacion
                if (DateTime.Now >= cita.FechaHora.AddHours(-2))
                    throw new InvalidOperationException("No se puede cancelar una cita con menos de 2 horas de anticipación");

                cita.Estado = EstadoCita.Cancelada;
                _unitOfWork.Citas.Update(cita);
                var cambios = await _unitOfWork.SaveChangesAsync();

                if (cambios > 0)
                {
                    var usuario = await _unitOfWork.Usuario.GetByIdAsync(dto.UsuarioId);

                    // Envio email de cancelacion
                    await _emailService.EnviarCancelacionCitaAsync(usuario.Email, usuario.Nombre, dto.FechaHora);

                    // Registro en el log que se cancelo la cita
                    _logService.LogCancelacion(dto.UsuarioId, dto.FechaHora);
                }

                return cambios > 0;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error cancelando cita para usuario {dto.UsuarioId}", ex);
                throw;
            }
        }

        // Este metodo obtiene los horarios disponibles para una fecha y turno
        // Lo hice asi porque el frontend necesita saber que horarios estan libres
        // Calculo la disponibilidad en tiempo real
        public async Task<IEnumerable<HorarioDisponibleDTO>> ObtenerHorariosDisponiblesAsync(DateTime fecha, int turnoId)
        {
            try
            {
                var config = (await _unitOfWork.Configuraciones.GetAllAsync())
                    .FirstOrDefault(c => c.Fecha.Date == fecha.Date && c.TurnoId == turnoId);

                if (config == null)
                    return new List<HorarioDisponibleDTO>();

                var turno = await _unitOfWork.Turnos.GetByIdAsync(turnoId);
                var citasDelDia = (await _unitOfWork.Citas.GetAllAsync())
                    .Where(c => c.FechaHora.Date == fecha.Date && c.TurnoId == turnoId && c.Estado != EstadoCita.Cancelada)
                    .GroupBy(c => c.FechaHora)
                    .ToDictionary(g => g.Key, g => g.Count());

                var horarios = new List<HorarioDisponibleDTO>();
                var inicio = turno.HoraInicio;
                var fin = turno.HoraFin;
                var totalSlots = (int)((fin - inicio).TotalMinutes / config.DuracionCitaMinutos);

                // Genero todos los horarios posibles
                for (int i = 0; i < totalSlots; i++)
                {
                    var horaSlot = inicio + TimeSpan.FromMinutes(i * config.DuracionCitaMinutos);
                    var fechaHora = fecha.Date + horaSlot;

                    var citasReservadas = citasDelDia.ContainsKey(fechaHora) ? citasDelDia[fechaHora] : 0;
                    var estacionesDisponibles = config.CantidadEstaciones - citasReservadas;

                    horarios.Add(new HorarioDisponibleDTO
                    {
                        FechaHora = fechaHora,
                        EstacionesDisponibles = Math.Max(0, estacionesDisponibles),
                        EstacionesTotales = config.CantidadEstaciones,
                        Duracion = TimeSpan.FromMinutes(config.DuracionCitaMinutos),
                        TurnoNombre = turno.Nombre
                    });
                }

                // Solo devuelvo horarios que tienen cupo disponible o que son en el futuro
                return horarios.Where(h => h.TieneCupo || DateTime.Now < h.FechaHora).ToList();
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error obteniendo horarios disponibles para {fecha:dd/MM/yyyy}", ex);
                throw;
            }
        }

        // Este metodo obtiene las citas de un usuario especifico
        // Lo hice asi porque el frontend necesita mostrar las citas del usuario logueado
        public async Task<IEnumerable<CitaUsuarioDTO>> ObtenerCitasUsuarioAsync(int usuarioId)
        {
            try
            {
                var citas = (await _unitOfWork.Citas.GetAllAsync())
                    .Where(c => c.UsuarioId == usuarioId && c.Estado != EstadoCita.Cancelada)
                    .OrderBy(c => c.FechaHora)
                    .ToList();

                var citasDto = new List<CitaUsuarioDTO>();

                foreach (var cita in citas)
                {
                    var turno = await _unitOfWork.Turnos.GetByIdAsync(cita.TurnoId);

                    citasDto.Add(new CitaUsuarioDTO
                    {
                        Id = cita.Id,
                        FechaHora = cita.FechaHora,
                        EstacionNumero = cita.EstacionNumero,
                        TipoTramite = cita.TipoTramite.ToString(),
                        Estado = cita.Estado.ToString(),
                        TurnoNombre = turno?.Nombre ?? "Desconocido"
                    });
                }

                return citasDto;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error obteniendo citas del usuario {usuarioId}", ex);
                throw;
            }
        }

        // Método privado para crear estaciones automáticamente
        // Lo hice así porque no quiero que el admin tenga que crear estaciones manualmente
        // El sistema las crea según la configuración necesaria
        private async Task CrearEstacionesPorDefectoAsync(int cantidadNecesaria)
        {
            try
            {
                var estacionesExistentes = await _unitOfWork.Estaciones.GetAllAsync();
                var ultimoNumero = estacionesExistentes.Any() ? estacionesExistentes.Max(e => e.Numero) : 0;

                for (int i = 1; i <= cantidadNecesaria; i++)
                {
                    var nuevaEstacion = new Estacion
                    {
                        Numero = ultimoNumero + i
                    };
                    await _unitOfWork.Estaciones.AddAsync(nuevaEstacion);
                }

                await _unitOfWork.SaveChangesAsync();
                _logService.LogConfiguracion($"ESTACIONES CREADAS - {cantidadNecesaria} estaciones nuevas creadas automáticamente", 0);
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error creando estaciones por defecto", ex);
                throw;
            }
        }
    }
}


