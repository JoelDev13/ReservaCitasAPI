using Domain.Entidades;
using Infrastructure.Persistencia.Contexto;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Infrastructure.Interfaces;

namespace ProyectoFinal.Infrastructure.Repositories
{
    public class CitaRepository : Repository<Cita>, ICitaRepository
    {
        public CitaRepository(CitasDbContext context) : base(context) { }

        public async Task<IEnumerable<Cita>> GetCitasPorPacienteAsync(int pacienteId)
        {
            return await _dbSet
                .Where(c => c.PacienteId == pacienteId)
                .ToListAsync();
        }
    }
}
