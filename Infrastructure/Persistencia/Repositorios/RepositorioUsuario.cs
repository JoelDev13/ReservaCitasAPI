using Domain.Entidades;
using Domain.Interfaces;
using Infrastructure.Persistencia.Contexto;
using System.Linq;

namespace Infrastructure.Persistencia.Repositorios
{
    public class RepositorioUsuario : IRepositorioUsuario
    {
        private readonly CitasDbContext _context;

        public RepositorioUsuario(CitasDbContext context)
        {
            _context = context;
        }

        public Usuario? ObtenerPorNombre(string nombre)
        {
            return _context.Usuarios.FirstOrDefault(u => u.Nombre == nombre);
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
