using Application.DTOs.Configuracion;
using Application.Interfaces;
using Application.Mappers;
using Domain.Interfaces;

namespace Application.Servicios
{
    public class ConfiguracionService : IConfiguracionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;

        public ConfiguracionService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork;
            _logService = logService;
        }

        public async Task<ConfiguracionDto> CrearConfiguracionAsync(ConfiguracionCreateDto dto)
        {
            // Valida que no exista configuracion para la misma fecha y turno
            var configuracionExistente = (await _unitOfWork.Configuraciones.GetAllAsync())
                .FirstOrDefault(c => c.Fecha.Date == dto.Fecha.Date && c.TurnoId == dto.TurnoId);

            if (configuracionExistente != null)
                throw new InvalidOperationException("Ya existe una configuracion para esta fecha y turno");

            // Validar que el turno existe
            var turno = await _unitOfWork.Turnos.GetByIdAsync(dto.TurnoId);
            if (turno == null)
                throw new InvalidOperationException("El turno especificado no existe");

            var entity = dto.ToEntity();
            await _unitOfWork.Configuraciones.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity.ToDto();
        }

        public async Task<IEnumerable<ConfiguracionDto>> ObtenerConfiguracionesActivasAsync()
        {
            try
            {
                return (await _unitOfWork.Configuraciones.GetAllAsync())
                .Where(c => c.Fecha.Date >= DateTime.Today)
                .Select(c => c.ToDto())
                .ToList();
            }
            catch (Exception ex)
            {
                _logService.LogError("Error obteniendo configuraciones activas", ex);
                throw;
            }
        }

        public async Task<bool> GenerarSlotsPorConfiguracionAsync(int configuracionId)
        {
            try
            {
                // Obtengo la configuracion
                var config = await _unitOfWork.Configuraciones.GetByIdAsync(configuracionId);
                if (config == null)
                    throw new InvalidOperationException("Configuracion no encontrada");

                // Obtengo el turno
                var turno = await _unitOfWork.Turnos.GetByIdAsync(config.TurnoId);
                if (turno == null)
                    throw new InvalidOperationException("Turno no encontrado");

                // Obtengo las estaciones
                var estaciones = await _unitOfWork.Estaciones.GetAllAsync();
                if (!estaciones.Any())
                    throw new InvalidOperationException("No hay estaciones disponibles");

                // Calculo cuantos slots caben en el turno
                var inicio = turno.HoraInicio;
                var fin = turno.HoraFin;
                var totalSlots = (int)((fin - inicio).TotalMinutes / config.DuracionCitaMinutos);
                var estacionesParaUsar = estaciones.Take(config.CantidadEstaciones).ToList();

                var citasAGuardar = new List<Domain.Entidades.Cita>();

                // Creo un slot por cada intervalo de tiempo
                for (int i = 0; i < totalSlots; i++)
                {
                    var horaSlot = inicio + TimeSpan.FromMinutes(i * config.DuracionCitaMinutos);

                    // Para cada estacion, creo una cita disponible
                    foreach (var estacion in estacionesParaUsar)
                    {
                        var cita = new Domain.Entidades.Cita
                        {
                            FechaHora = config.Fecha.Date + horaSlot,
                            EstacionNumero = estacion.Numero,
                            Estado = Domain.Enums.EstadoCita.Pendiente,
                            TurnoId = config.TurnoId,
                            UsuarioId = 0, // 0 significa que no hay usuario asignado
                            TipoTramite = Domain.Enums.TipoTramite.Ninguno
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
                
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error generando slots para configuracion {configuracionId}", ex);
                throw;
            }
        }

        public async Task<bool> EliminarConfiguracionAsync(int configuracionId)
        {
            try
            {
                // Obtengo la configuracion
                var config = await _unitOfWork.Configuraciones.GetByIdAsync(configuracionId);
                if (config == null)
                    return false;

                // Valido que no haya citas activas para esta configuracion
                var citasActivas = (await _unitOfWork.Citas.GetAllAsync())
                    .Where(c => c.FechaHora.Date == config.Fecha.Date && 
                               c.TurnoId == config.TurnoId && 
                               c.Estado != Domain.Enums.EstadoCita.Cancelada)
                    .ToList();

                if (citasActivas.Any())
                {
                    throw new InvalidOperationException("No se puede eliminar la configuración porque hay citas activas para esta fecha y turno");
                }

                // Elimino la configuracion
                _unitOfWork.Configuraciones.Delete(config);
                await _unitOfWork.SaveChangesAsync();

                // Registro en el log que se eliminó la configuracion
                _logService.LogConfiguracion($"CONFIGURACIÓN ELIMINADA - Fecha {config.Fecha:dd/MM/yyyy}, Turno {config.TurnoId}", configuracionId);
                
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error eliminando configuración {configuracionId}", ex);
                throw;
            }
        }
    }
}

