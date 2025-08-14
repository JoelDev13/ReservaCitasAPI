using Application.DTOs.Cita;

namespace Application.Interfaces
{
    public interface ICitaService
    {
        bool ReservarCita(ReservarCitaDTO dto);
    }
}
