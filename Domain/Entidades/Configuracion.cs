using Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entidades
{
    public class Configuracion : IEntity
    {
        public int Id { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public int DuracionCitaMinutos { get; set; }

        [Required]
        public int CantidadEstaciones { get; set; }

        // Relacion con Turno
        public int TurnoId { get; set; }
        public Turno Turno { get; set; }
    }
}
