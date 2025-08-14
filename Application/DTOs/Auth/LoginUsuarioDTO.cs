using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public class LoginUsuarioDTO
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string ContrasenaHash { get; set; } = string.Empty;
    }
}
