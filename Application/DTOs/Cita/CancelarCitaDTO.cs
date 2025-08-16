using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Cita
{
    public class CancelarCitaDTO
    {
        [Required(ErrorMessage = "El ID de usuario es obligatorio")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "La fecha y hora de la cita son obligatorias")]
        public DateTime FechaHora { get; set; }

        public string SoloHora => FechaHora.ToString("HH:mm");
        public string FechaCompleta => FechaHora.ToString("dd/MM/yyyy");
    }
}
