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

        [Required(ErrorMessage = "La cedula es obligatoria")]
        [StringLength(20, MinimumLength = 11, ErrorMessage = "La cedula debe tener al menos 11 digitos")]
        [RegularExpression(@"^\d{11,}$", ErrorMessage = "La cedula debe contener solo numeros y tener al menos 11 digitos")]
        public string Cedula { get; set; } = string.Empty;

        [Required]
        public RolUsuario RolUsuario { get; set; }
    }
}
