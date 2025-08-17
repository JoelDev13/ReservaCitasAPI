using Domain.Enums;
using Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entidades
{
    public class Usuario : IEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }  = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string ContrasenaHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cedula es obligatoria")]
        [StringLength(20, MinimumLength = 11, ErrorMessage = "La cedula debe tener al menos 11 dígitos")]
        [RegularExpression(@"^\d{11,}$", ErrorMessage = "La cedula debe contener solo numeros y tener al menos 11 digitos")]
        public string Cedula { get; set; } = string.Empty;

        public RolUsuario Rol { get; set; } = RolUsuario.Usuario;

        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}
