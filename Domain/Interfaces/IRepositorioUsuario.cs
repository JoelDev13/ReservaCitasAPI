using Domain.Entidades;

namespace Domain.Interfaces
{
    public interface IRepositorioUsuario
    {
        Usuario? ObtenerPorNombre(string nombreUsuario);
        void Agregar(Usuario usuario);
        bool GuardarCambios();
    }
}
