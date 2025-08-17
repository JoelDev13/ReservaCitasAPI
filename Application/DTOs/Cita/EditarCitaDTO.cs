using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Cita
{
    public class EditarCitaDTO
    {
        [Required]
        public int CitaId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public DateTime NuevaFechaHora { get; set; }

        public TipoTramite? NuevoTipoTramite { get; set; }
    }
}
