namespace Application.DTOs.Auth
{
    public class RespuestaLoginDTO
    {
        public string Token { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public int Rol { get; set; }
    }
}
