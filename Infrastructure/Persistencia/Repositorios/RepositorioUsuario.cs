using Domain.Entidades;
using Domain.Interfaces;
using Infrastructure.Persistencia.Contexto;
using ProyectoFinal.Infrastructure.Repositories;
using System.Linq;

namespace Infrastructure.Persistencia.Repositorios
{
    public class RepositorioUsuario : GenericRepository<Usuario>, IRepositorioUsuario
    {
        private readonly CitasDbContext _context;

        public RepositorioUsuario(CitasDbContext context) : base(context)
        {
            _context = context;
        }

        public Usuario? ObtenerPorEmail(string email)
        {
            return _context.Usuarios.FirstOrDefault(u => u.Email == email);
        }

        public Usuario? ObtenerPorCedula(string cedula)
        {
            return _context.Usuarios.FirstOrDefault(u => u.Cedula == cedula);
        }

        public void Agregar(Usuario entity)
        {
            _context.Usuarios.Add(entity);
        }

        public bool GuardarCambios()
        {
            return _context.SaveChanges() > 0;
        }

    }
}
