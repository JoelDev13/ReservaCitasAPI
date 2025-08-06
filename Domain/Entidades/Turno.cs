using Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entidades
{
    public class Turno : IEntity
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } // Ej. Matutino

        [Required]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        public TimeSpan HoraFin { get; set; }

        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
        public ICollection<Configuracion> Configuraciones { get; set; } = new List<Configuracion>();
    }
}
