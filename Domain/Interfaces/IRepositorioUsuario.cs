using Domain.Entidades;

namespace Domain.Interfaces
{
    public interface IRepositorioUsuario
    {
        Usuario? ObtenerPorEmail(string email);
        Usuario? ObtenerPorCedula(string cedula);
        void Agregar(Usuario usuario);
        bool GuardarCambios();
    }
}
