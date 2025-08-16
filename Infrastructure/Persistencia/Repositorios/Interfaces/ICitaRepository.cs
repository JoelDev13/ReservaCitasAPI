using Domain.Entidades;
using ProyectoFinal.Domain.Interfaces;

namespace Infrastructure.Persistencia.Repositorios.Interfaces
{
    public interface ICitaRepository : IGenericRepository<Cita>
    {
        Task<IEnumerable<Cita>> GetCitasPorUsuarioAsync(int usuarioId);
        Task<IEnumerable<Cita>> GetCitasPorFechaYTurnoAsync(DateTime fecha, int turnoId);
    }
}
