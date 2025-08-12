using Domain.Entidades;
using ProyectoFinal.Domain.Interfaces;

namespace ProyectoFinal.Infrastructure.Interfaces
{
    public interface ICitaRepository : IGenericRepository<Cita>
    {
        Task<IEnumerable<Cita>> GetCitasPorUsuarioAsync(int usuarioId);
    }
}
