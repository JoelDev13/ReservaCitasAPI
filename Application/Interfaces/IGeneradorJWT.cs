namespace Application.Interfaces
{
    public interface IGeneradorJWT
    {
        string GenerarToken(Domain.Entidades.Usuario usuario);
    }
}
