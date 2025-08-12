using Application.Interfaces;
using Application.DTOs.Configuracion;

namespace Application.Interfaces
{
    public interface IConfiguracionService
    {
        Task<ConfiguracionDto> CrearConfiguracionAsync(ConfiguracionCreateDto dto);
        Task<IEnumerable<ConfiguracionDto>> ObtenerConfiguracionesActivasAsync();
    }
}
