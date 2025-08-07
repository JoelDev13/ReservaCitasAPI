using Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistencia.Contexto
{
    public class CitasDbContext : DbContext
    {
        public CitasDbContext(DbContextOptions<CitasDbContext> options) : base(options)
        {
        }


        public DbSet<Usuario> Usuarios { get; set;}
        public DbSet<Cita> Citas { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<Configuracion> Configuraciones { get; set; }
        public DbSet<Estacion> Estaciones { get; set; }
        
    }
}
