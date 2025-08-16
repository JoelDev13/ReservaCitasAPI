using Application.DTOs.Cita;

namespace Application.Interfaces
{
    public interface ICitaService
    {
        Task GenerarSlotsPorConfiguracionAsync(int configuracionId);
        Task<bool> ReservarCitaAsync(ReservarCitaDTO dto);
        Task<bool> CancelarCitaAsync(CancelarCitaDTO dto);
        Task<IEnumerable<HorarioDisponibleDTO>> ObtenerHorariosDisponiblesAsync(DateTime fecha, int turnoId);
        Task<IEnumerable<CitaUsuarioDTO>> ObtenerCitasUsuarioAsync(int usuarioId);
    }
}
