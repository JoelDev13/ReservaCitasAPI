using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Cita
{
    public class ReservarCitaDTO
    {
        [Required]
        public int UsuarioId { get; set; }  

        [Required]
        public DateTime FechaHora { get; set; } 

        [Required]
        public int? EstacionNumero { get; set; }

        [Required]
        public TipoTramite TipoTramite { get; set; }
        public int TurnoId { get; set; }


    }
}
