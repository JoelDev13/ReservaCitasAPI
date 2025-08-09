using Domain.Entidades;
using ProyectoFinal.Domain.Interfaces;


namespace Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Usuario> Usuario { get; }
        IRepository<Cita> Citas { get; }
        IRepository<Turno> Turnos { get; }
        IRepository<Configuracion> Configuraciones { get; }
        IRepository<Estacion> Estaciones { get; }
        Task<int>SaveChangesAsync();
        
    }
   
    
}
