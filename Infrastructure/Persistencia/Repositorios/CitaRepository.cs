using Domain.Entidades;
using Infrastructure.Persistencia.Contexto;
using Infrastructure.Persistencia.Repositorios.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ProyectoFinal.Infrastructure.Repositories
{
    public class CitaRepository : GenericRepository<Cita>, ICitaRepository
    {
        public CitaRepository(CitasDbContext context) : base(context) { }

        public async Task<IEnumerable<Cita>> GetCitasPorUsuarioAsync(int usuarioId)
        {
            return await _dbSet
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();
        }


    }
}
