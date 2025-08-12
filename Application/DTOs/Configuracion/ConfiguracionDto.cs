namespace Application.DTOs.Configuracion
{
    public class ConfiguracionDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int DuracionCitaMinutos { get; set; }
        public int CantidadEstaciones { get; set; }
        public int TurnoId { get; set; }
    }
}
