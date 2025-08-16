namespace Application.Interfaces
{
    public interface ILogService
    {
        void LogReserva(int usuarioId, DateTime fechaHora, int estacion);
        void LogCancelacion(int usuarioId, DateTime fechaHora);
        void LogLogin(string email);
        void LogError(string mensaje, Exception ex = null);
        void LogConfiguracion(string accion, int configuracionId);
    }
}
