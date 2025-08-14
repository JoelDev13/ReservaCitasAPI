using Domain.Entidades;

namespace Domain.Interfaces
{
    public interface IRepositorioUsuario
    {
        Usuario? ObtenerPorEmail(string email);
        void Agregar(Usuario usuario);
        bool GuardarCambios();
    }
}
