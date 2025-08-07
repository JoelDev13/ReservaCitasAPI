using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistencia.Contexto
{
    public class CitasDbContextFactory : IDesignTimeDbContextFactory<CitasDbContext>
    {
        public CitasDbContext CreateDbContext(string[] args)
        {
            // Debido a que el proyecto Infrastructure esta separado de otras capas (Onion),
            // tengo que crear esta clase para que EF Core pueda crear una instancia del DbContext
            // con la cadena de conexión en tiempo de diseño (migraciones)

            var optionsBuilder = new DbContextOptionsBuilder<CitasDbContext>();
            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=3432;Database=SistemaCitaDb;Username=admin;Password=TuPasswordSeguraJM",
                b => b.MigrationsAssembly("Infrastructure")

                );

            return new CitasDbContext(optionsBuilder.Options);
        }
    }
}
