using Application.DTOs.Configuracion;
using Application.Interfaces;
using Application.Mappers;
using Domain.Interfaces;

namespace Application.Servicios
{
    public class ConfiguracionService : IConfiguracionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ConfiguracionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ConfiguracionDto> CrearConfiguracionAsync(ConfiguracionCreateDto dto)
        {
            //Valida duplicacion
            var configuracionesExistentes = await _unitOfWork.Configuraciones.GetAllAsync();
            bool esDuplicada = configuracionesExistentes.Any(c =>
                c.Fecha.Date == dto.Fecha.Date &&
                c.TurnoId == dto.TurnoId
            );

            if (esDuplicada)
                throw new InvalidOperationException("Ya existe una configuracion para esta fecha y turno");

            var entity = dto.ToEntity();
            await _unitOfWork.Configuraciones.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity.ToDto();
        }

        public async Task<IEnumerable<ConfiguracionDto>> ObtenerConfiguracionesActivasAsync()
        {
            var configuraciones = await _unitOfWork.Configuraciones.GetAllAsync();
            return configuraciones
                .Where(c => c.Fecha.Date >= DateTime.Today)
                .Select(c => c.ToDto());
        }
    }
}

