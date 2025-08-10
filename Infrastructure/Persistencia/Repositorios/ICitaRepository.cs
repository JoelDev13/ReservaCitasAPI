using Domain.Entidades;
using ProyectoFinal.Domain.Interfaces;

namespace ProyectoFinal.Infrastructure.Interfaces
{
    public interface ICitaRepository : IRepository<Cita>
    {
        Task<IEnumerable<Cita>> GetCitasPorUsuarioAsync(int usuarioId);
    }
}
