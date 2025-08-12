using Application.DTOs.Configuracion;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Configuracion
{
    public class ConfiguracionCreateDto
    {
        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "La duracion de la cita es obligatoria")]
        [Range(1, 240, ErrorMessage = "La duracion debe ser entre 1 y 240 minutos")]
        public int DuracionCitaMinutos { get; set; }

        [Required(ErrorMessage = "La cantidad de estaciones es obligatoria")]
        [Range(1, 50, ErrorMessage = "Debe haber al menos 1 estación")]
        public int CantidadEstaciones { get; set; }

        [Required(ErrorMessage = "El turno es obligatorio")]
        public int TurnoId { get; set; }
    }
}
