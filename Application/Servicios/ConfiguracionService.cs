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

        // Metodo para obtener todos los turnos disponibles
        public async Task<IEnumerable<Domain.Entidades.Turno>> ObtenerTurnosAsync()
        {
            try
            {
                return await _unitOfWork.Turnos.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logService.LogError("Error obteniendo turnos", ex);
                throw;
            }
        }

        public async Task<bool> GenerarSlotsPorConfiguracionAsync(int configuracionId)
        {
            try
            {
                // Este metodo ahora está centralizado en CitaService
                // para evitar duplicación de lógica de negocio
                // La generación de slots se maneja desde el CitaService
                // que tiene acceso a toda la lógica necesaria
                
                _logService.LogConfiguracion($"GENERACIÓN DE SLOTS SOLICITADA - Configuración {configuracionId}", configuracionId);
                
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error en solicitud de generación de slots para configuración {configuracionId}", ex);
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

