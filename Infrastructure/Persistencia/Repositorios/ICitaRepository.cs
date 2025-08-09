using Domain.Entidades;

namespace ProyectoFinal.Infrastructure.Interfaces
{
    public interface ICitaRepository : IRepository<Cita>
    {
        Task<IEnumerable<Cita>> GetCitasPorPacienteAsync(int pacienteId);
    }
}
