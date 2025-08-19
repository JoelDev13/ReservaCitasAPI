using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Domain.Enums;
using Application.Converters;

namespace Application.DTOs.Cita
{
    public class ReservarCitaDTO
    {
        [Required]
        public int UsuarioId { get; set; }  

        [Required]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime FechaHora { get; set; } 

        public int? EstacionNumero { get; set; }

        [Required]
        public TipoTramite TipoTramite { get; set; }
        
        [Required]
        public int TurnoId { get; set; }
    }
}
