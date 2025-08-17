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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Nombre)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(150);
                
                entity.Property(u => u.Cedula)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.HasIndex(u => u.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Usuario_Email");

                //Indice unico para la cedula
                 entity.HasIndex(u => u.Cedula)
                       .IsUnique()
                       .HasDatabaseName("IX_Usuario_Cedula");

                entity.Property(u => u.ContrasenaHash)
                    .IsRequired()
                    .HasMaxLength(255);

                // RELACION 1:N - Usuario tiene muchas Citas
                entity.HasMany(u => u.Citas)
                    .WithOne(c => c.Usuario)
                    .HasForeignKey(c => c.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Turno>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Nombre)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(t => t.HoraInicio)
                    .IsRequired();

                entity.Property(t => t.HoraFin)
                    .IsRequired();

                // RELACION 1:N - Turno tiene muchas Citas
                entity.HasMany(t => t.Citas)
                    .WithOne(c => c.Turno)
                    .HasForeignKey(c => c.TurnoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // RELACION 1:N - Turno tiene muchas Configuraciones
                entity.HasMany(t => t.Configuraciones)
                    .WithOne(c => c.Turno)
                    .HasForeignKey(c => c.TurnoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Estacion>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Numero)
                    .IsRequired();

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(200);

                entity.HasIndex(e => e.Numero)
                    .IsUnique()
                    .HasDatabaseName("IX_Estacion_Numero");
            });


            modelBuilder.Entity<Configuracion>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Fecha)
                    .IsRequired()
                    .HasColumnType("date");

                entity.Property(c => c.DuracionCitaMinutos)
                    .IsRequired();

                entity.Property(c => c.CantidadEstaciones)
                    .IsRequired();

                // RELACION N:1 - Muchas Configuraciones pertenecen a un Turno
                entity.HasOne(c => c.Turno)
                    .WithMany(t => t.Configuraciones)
                    .HasForeignKey(c => c.TurnoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indice compuesto para evitar configuraciones duplicadas
                entity.HasIndex(c => new { c.Fecha, c.TurnoId })
                    .IsUnique()
                    .HasDatabaseName("IX_Configuracion_Fecha_Turno");
            });


            modelBuilder.Entity<Cita>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.FechaHora)
                    .IsRequired()
                    .HasColumnType("timestamp");

                entity.Property(c => c.EstacionNumero)
                    .IsRequired();

                entity.Property(c => c.Estado)
                    .IsRequired()
                    .HasConversion<int>() // Convierte enum EstadoCita a int
                    .HasComment("0=Ninguno, 1=Renovacion, 2=PrimeraVez, 3=Duplicado, 4=CambioCategoria\"");

                entity.Property(c => c.TipoTramite)
                    .IsRequired()
                    .HasConversion<int>() // guarda el nombre del enum como int
                    .HasMaxLength(50)
                    .HasComment("Renovacion, PrimeraVez, Duplicado, CambioCategoria");

                // RELACION N:1 - Muchas Citas pertenecen a un Usuario
                entity.HasOne(c => c.Usuario)
                    .WithMany(u => u.Citas)
                    .HasForeignKey(c => c.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                // RELACION N:1 - Muchas Citas pertenecen a un Turno
                entity.HasOne(c => c.Turno)
                    .WithMany(t => t.Citas)
                    .HasForeignKey(c => c.TurnoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indice compuesto para evitar mUltiples citas en el mismo slot de tiempo/estaciÃ³n
                entity.HasIndex(c => new { c.FechaHora, c.EstacionNumero })
                    .IsUnique()
                    .HasDatabaseName("IX_Cita_FechaHora_Estacion");

                // Indice para busquedas por usuario y fecha
                entity.HasIndex(c => new { c.UsuarioId, c.FechaHora })
                    .HasDatabaseName("IX_Cita_Usuario_Fecha");

                // Indice para busquedas por estado
                entity.HasIndex(c => c.Estado)
                    .HasDatabaseName("IX_Cita_Estado");
            });

            base.OnModelCreating(modelBuilder);
        }

    }
}
