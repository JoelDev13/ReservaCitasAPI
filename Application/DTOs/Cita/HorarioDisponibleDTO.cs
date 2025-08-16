namespace Application.DTOs.Cita
{
    public class HorarioDisponibleDTO
    {
        public DateTime FechaHora { get; set; }
        public int EstacionesDisponibles { get; set; }
        public int EstacionesTotales { get; set; }
        public bool TieneCupo => EstacionesDisponibles > 0;
        public TimeSpan Duracion { get; set; }
        public string TurnoNombre { get; set; } = string.Empty;

        // formateo de fecha
        public string SoloHora => FechaHora.ToString("HH:mm");
        public string FechaCompleta => FechaHora.ToString("dd/MM/yyyy");
    }
}
