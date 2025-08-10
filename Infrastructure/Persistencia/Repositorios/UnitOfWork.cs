
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

        private IRepository<Usuario> _usuarios;

        private IRepository<Cita> _citas;

        private IRepository<Turno> _turnos;

        private IRepository<Configuracion> _configuraciones;

        private IRepository<Estacion> _estaciones;

        public UnitOfWork(CitasDbContext context)

        {

            _context = context;

        }
        public IRepository<Usuario> Usuario => _usuarios?? new GenericRepository<Usuario>(_context);

        public IRepository<Cita> Citas => _citas ??= new GenericRepository<Cita>(_context);

        public IRepository<Turno> Turnos => _turnos ??= new GenericRepository<Turno>(_context);

        public IRepository<Configuracion> Configuraciones => _configuraciones ??= new GenericRepository<Configuracion>(_context);

        public IRepository<Estacion> Estaciones => _estaciones ??= new GenericRepository<Estacion>(_context);


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
