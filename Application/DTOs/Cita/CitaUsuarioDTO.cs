namespace Application.DTOs.Cita
{
    public class CitaUsuarioDTO
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public int EstacionNumero { get; set; }
        public string TipoTramite { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string TurnoNombre { get; set; } = string.Empty;

        // Info formateada para mostrar
        public string FechaFormateada => FechaHora.ToString("dd/MM/yyyy");
        public string HorarioFormateado => FechaHora.ToString("HH:mm");
        public bool PuedeCancelar => DateTime.Now < FechaHora.AddHours(-2); // No cancelar 2h antes
    }
}
