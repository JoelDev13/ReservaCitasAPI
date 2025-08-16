using Application.Interfaces;

namespace Infrastructure.Servicios
{
    public sealed class LogService : ILogService
    {
        private static LogService _instance;
        private static readonly object _lock = new object();
        private readonly string _logPath;

        private LogService()
        {
            _logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "sistema.log");
            Directory.CreateDirectory(Path.GetDirectoryName(_logPath));
        }

        public static LogService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new LogService();
                    }
                }
                return _instance;
            }
        }

        public void LogReserva(int usuarioId, DateTime fechaHora, int estacion)
        {
            EscribirLog($"RESERVA - Usuario {usuarioId} reservo cita para {fechaHora:dd/MM/yyyy HH:mm} en estacion {estacion}");
        }

        public void LogCancelacion(int usuarioId, DateTime fechaHora)
        {
            EscribirLog($"CANCELACION - Usuario {usuarioId} canceló cita del {fechaHora:dd/MM/yyyy HH:mm}");
        }

        public void LogLogin(string email)
        {
            EscribirLog($"LOGIN - Usuario {email} inició sesión");
        }

        public void LogError(string mensaje, Exception ex = null)
        {
            var log = $"ERROR - {mensaje}";
            if (ex != null) log += $" | Exception: {ex.Message}";
            EscribirLog(log);
        }

        public void LogConfiguracion(string accion, int configuracionId)
        {
            EscribirLog($"CONFIGURACION - {accion} configuración ID: {configuracionId}");
        }

        private void EscribirLog(string mensaje)
        {
            try
            {
                var linea = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {mensaje}";
                File.AppendAllText(_logPath, linea + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // En caso de error escribiendo log, usar Console como fallback
                Console.WriteLine($"Error escribiendo log: {ex.Message}");
                Console.WriteLine($"Log original: {mensaje}");
            }
        }

    }
}
