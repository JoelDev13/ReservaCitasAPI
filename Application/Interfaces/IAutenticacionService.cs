using Application.DTOs.Auth;

namespace Infrastructure.Persistencia.Repositorios.Interfaces
{
    public interface IAutenticacionService
    {
        bool Registrar(RegistroUsuarioDTO dto);
        RespuestaLoginDTO? Login(LoginUsuarioDTO dto);
    }
}
