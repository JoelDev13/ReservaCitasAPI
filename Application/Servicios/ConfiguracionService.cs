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
    }
}

