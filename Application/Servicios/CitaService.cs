using Application.DTOs.Cita;
using Application.Interfaces;
using Domain.Entidades;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Servicios
{
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

        // Genera los slots segun la configuracon
        public async Task GenerarSlotsPorConfiguracionAsync(int configuracionId)
        {
            try
            {
                var config = await _unitOfWork.Configuraciones.GetByIdAsync(configuracionId);
                if (config == null)
                    throw new Exception("Configuracion no encontrada");

                var turno = await _unitOfWork.Turnos.GetByIdAsync(config.TurnoId);
                if (turno == null)
                    throw new Exception("Turno no encontrado");

                var estaciones = await _unitOfWork.Estaciones.GetAllAsync();
                if (!estaciones.Any())
                    throw new Exception("No hay estaciones disponibles");

                var inicio = turno.HoraInicio;
                var fin = turno.HoraFin;
                var totalSlots = (int)((fin - inicio).TotalMinutes / config.DuracionCitaMinutos);
                var estacionesParaUsar = estaciones.Take(config.CantidadEstaciones).ToList();

                var citasAGuardar = new List<Cita>();

                for (int i = 0; i < totalSlots; i++)
                {
                    var horaSlot = inicio + TimeSpan.FromMinutes(i * config.DuracionCitaMinutos);

                    foreach (var estacion in estacionesParaUsar)
                    {
                        var cita = new Cita
                        {
                            FechaHora = config.Fecha.Date + horaSlot,
                            EstacionNumero = estacion.Numero,
                            Estado = EstadoCita.Pendiente,
                            TurnoId = config.TurnoId,
                            UsuarioId = 0,
                            TipoTramite = TipoTramite.Ninguno
                        };
                        citasAGuardar.Add(cita);
                    }
                }

                foreach (var cita in citasAGuardar)
                {
                    await _unitOfWork.Citas.AddAsync(cita);
                }
                await _unitOfWork.SaveChangesAsync();

                _logService.LogConfiguracion($"SLOTS GENERADOS - {citasAGuardar.Count} slots", configuracionId);
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error generando slots para configuracion {configuracionId}", ex);
                throw;
            }
        }

        public async Task<bool> ReservarCitaAsync(ReservarCitaDTO dto)
        {
            try
            {
                //Solo 1 cita activa por dia por usuario
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

                //Configuracion activa
                var config = (await _unitOfWork.Configuraciones.GetAllAsync())
                    .FirstOrDefault(c => c.Fecha.Date == dto.FechaHora.Date && c.TurnoId == dto.TurnoId);

                if (config == null)
                    throw new InvalidOperationException("No hay configuracion activa para la fecha/turno seleccionado");

                //Horario valido
                var turno = await _unitOfWork.Turnos.GetByIdAsync(config.TurnoId);
                if (dto.FechaHora.TimeOfDay < turno.HoraInicio || dto.FechaHora.TimeOfDay >= turno.HoraFin)
                    throw new InvalidOperationException("La fecha/hora esta fuera del horario permitido");

                //Control de concurrencia y cupos
                var citasExistentes = (await _unitOfWork.Citas.GetAllAsync())
                    .Where(c => c.FechaHora == dto.FechaHora && c.Estado != EstadoCita.Cancelada)
                    .ToList();

                if (citasExistentes.Count >= config.CantidadEstaciones)
                    throw new InvalidOperationException("No hay cupo disponible en el horario seleccionado");

                //Estacion disponible
                var estaciones = await _unitOfWork.Estaciones.GetAllAsync();
                var estacionesOcupadas = citasExistentes.Select(c => c.EstacionNumero).ToList();
                var estacionLibre = estaciones.FirstOrDefault(e => !estacionesOcupadas.Contains(e.Numero));

                if (estacionLibre == null)
                    throw new InvalidOperationException("No hay estaciones libres en el horario solicitado");

                // Crear cita
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


                // Enviar email
                if (cambios > 0)
                {
                    var usuario = await _unitOfWork.Usuario.GetByIdAsync(dto.UsuarioId);

                    // Enviar email de confirmacion
                    await _emailService.EnviarConfirmacionCitaAsync(
                        usuario.Email,
                        usuario.Nombre,
                        dto.FechaHora,
                        cita.EstacionNumero,
                        "Confirmada");

                    // Log de la reserva
                    _logService.LogReserva(dto.UsuarioId, dto.FechaHora, cita.EstacionNumero);
                }

                return cambios > 0;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error reservando cita para usuario {dto.UsuarioId}", ex);
                throw;
            }
        }

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

                // Valida que se pueda cancelar (minimo 2 horas antes)
                if (DateTime.Now >= cita.FechaHora.AddHours(-2))
                    throw new InvalidOperationException("No se puede cancelar una cita con menos de 2 horas de anticipación");

                cita.Estado = EstadoCita.Cancelada;
                _unitOfWork.Citas.Update(cita);
                var cambios = await _unitOfWork.SaveChangesAsync();

                if (cambios > 0)
                {
                    var usuario = await _unitOfWork.Usuario.GetByIdAsync(dto.UsuarioId);

                    //Envia el email de cancelacion
                    await _emailService.EnviarCancelacionCitaAsync(usuario.Email, usuario.Nombre, dto.FechaHora);

                    //Log de la cancelacion
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

                return horarios.Where(h => h.TieneCupo || DateTime.Now < h.FechaHora).ToList();
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error obteniendo horarios disponibles para {fecha:dd/MM/yyyy}", ex);
                throw;
            }
        }

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


    }
}


