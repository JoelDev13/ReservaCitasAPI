using Application.Interfaces;
using Application.DTOs.Configuracion;
using Domain.Entidades;

namespace Application.Interfaces
{
    public interface IConfiguracionService
    {
        Task<ConfiguracionDto> CrearConfiguracionAsync(ConfiguracionCreateDto dto);
        Task<IEnumerable<ConfiguracionDto>> ObtenerConfiguracionesActivasAsync();
        Task<bool> GenerarSlotsPorConfiguracionAsync(int configuracionId);
        Task<bool> EliminarConfiguracionAsync(int configuracionId);
        Task<IEnumerable<Turno>> ObtenerTurnosAsync();
    }
}
