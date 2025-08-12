using Domain.Entidades;
using ProyectoFinal.Domain.Interfaces;


namespace Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Usuario> Usuario { get; }
        IGenericRepository<Cita> Citas { get; }
        IGenericRepository<Turno> Turnos { get; }
        IGenericRepository<Configuracion> Configuraciones { get; }
        IGenericRepository<Estacion> Estaciones { get; }
        

        Task<int>SaveChangesAsync();
        
    }
   
    
}
