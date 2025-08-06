using Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entidades
{
    public class Usuario : IEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string ContrasenaHash { get; set; }

        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}
