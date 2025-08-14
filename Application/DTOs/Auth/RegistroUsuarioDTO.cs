using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public class RegistroUsuarioDTO
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato valido")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string ContrasenaHash { get; set; } = string.Empty;

        [Required]
        public RolUsuario RolUsuario { get; set; }
    }
}
