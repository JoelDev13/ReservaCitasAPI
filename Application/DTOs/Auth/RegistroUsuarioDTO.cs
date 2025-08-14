using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public class RegistroUsuarioDTO
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [Required]
        public string ContrasenaHash { get; set; } = string.Empty;

        [Required]
        public RolUsuario RolUsuario { get; set; }
    }
}
