using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Application.Converters;

namespace Application.DTOs.Cita
{
    public class EditarCitaDTO
    {
        [Required]
        public int CitaId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime NuevaFechaHora { get; set; }

        public TipoTramite? NuevoTipoTramite { get; set; }
    }
}
