using Domain.Enums;
using Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entidades
{
    public class Cita : IEntity
    {
        public int Id { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        [Required]
        public int EstacionNumero { get; set; }

        [Required]
        public TipoTramite Estado { get; set; }

        // Relaciones
        [Required]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        [Required]
        public int TurnoId { get; set; }
        public Turno Turno
        {
            get; set;
        }
        public string TipoTramite { get; set; } //renovacion, primera vez, etc..
    }
}
