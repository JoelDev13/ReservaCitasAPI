
using Domain.Entidades;
using Domain.Interfaces;
using Infrastructure.Persistencia.Contexto;
using ProyectoFinal.Domain.Interfaces;
using ProyectoFinal.Infrastructure.Repositories;

namespace Infrastructure.Persistencia.Repositorios
{
    public class UnitOfWork : IUnitOfWork

    {

        private readonly CitasDbContext _context;

        private IGenericRepository<Usuario> _usuarios; 

        private IGenericRepository<Cita> _citas;

        private IGenericRepository<Turno> _turnos;

        private IGenericRepository<Configuracion> _configuraciones;

        private IGenericRepository<Estacion> _estaciones;

        public UnitOfWork(CitasDbContext context)

        {

            _context = context;
            

        }
        public IGenericRepository<Usuario> Usuario => _usuarios?? new GenericRepository<Usuario>(_context);

        public IGenericRepository<Cita> Citas => _citas ??= new GenericRepository<Cita>(_context);

        public IGenericRepository<Turno> Turnos => _turnos ??= new GenericRepository<Turno>(_context);

        public IGenericRepository<Configuracion> Configuraciones => _configuraciones ??= new GenericRepository<Configuracion>(_context);

        public IGenericRepository<Estacion> Estaciones => _estaciones ??= new GenericRepository<Estacion>(_context);


        public async Task<int> SaveChangesAsync()

        {

            return await _context.SaveChangesAsync();

        }

        public void Dispose()

        {

            _context.Dispose();

        }

    }

}
