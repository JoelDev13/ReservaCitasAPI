using Application.Interfaces;

namespace Infrastructure.Servicios
{
    // Este es mi LogService que implementa el patron Singleton
    // Lo hice asi porque necesito que solo haya una instancia del servicio de logging en toda la aplicacion
    // Si no fuera Singleton, podria tener multiples archivos de log y seria un desorden
    public sealed class LogService : ILogService
    {
        // Variable estatica que guarda la unica instancia
        private static LogService _instance;
        
        // Lock object para hacer el Singleton thread-safe
        // Esto es importante porque si dos hilos intentan crear la instancia al mismo tiempo, podria haber problemas
        private static readonly object _lock = new object();
        
        // Ruta donde se guardan los logs
        private readonly string _logPath;

        // Constructor privado - solo se puede llamar desde dentro de la clase
        // Esto es parte del patron Singleton
        private LogService()
        {
            // Creo la carpeta logs si no existe
            _logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "sistema.log");
            Directory.CreateDirectory(Path.GetDirectoryName(_logPath));
        }

        // Propiedad estatica que devuelve la instancia
        // Aqui es donde implemento el patron Singleton
        public static LogService Instance
        {
            get
            {
                // Si no hay instancia, la creo
                if (_instance == null)
                {
                    // Uso lock para evitar problemas de concurrencia
                    lock (_lock)
                    {
                        // Verifico otra vez por si otro hilo ya creo la instancia
                        if (_instance == null)
                            _instance = new LogService();
                    }
                }
                return _instance;
            }
        }

        // Metodo para registrar reservas exitosas
        // Lo uso cuando un usuario reserva una cita correctamente
        public void LogReserva(int usuarioId, DateTime fechaHora, int estacion)
        {
            EscribirLog($"RESERVA - Usuario {usuarioId} reservo cita para {fechaHora:dd/MM/yyyy HH:mm} en estacion {estacion}");
        }

        // Metodo para registrar cancelaciones
        // Lo uso cuando un usuario cancela su cita
        public void LogCancelacion(int usuarioId, DateTime fechaHora)
        {
            EscribirLog($"CANCELACION - Usuario {usuarioId} canceló cita del {fechaHora:dd/MM/yyyy HH:mm}");
        }

        // Metodo para registrar logins
        // Lo uso cuando alguien inicia sesion
        public void LogLogin(string email)
        {
            EscribirLog($"LOGIN - Usuario {email} inició sesión");
        }

        // Metodo para registrar errores
        // Lo uso cuando algo sale mal en el sistema
        public void LogError(string mensaje, Exception ex = null)
        {
            var log = $"ERROR - {mensaje}";
            if (ex != null) log += $" | Exception: {ex.Message}";
            EscribirLog(log);
        }

        // Metodo para registrar cambios de configuracion
        // Lo uso cuando el admin cambia algo en el sistema
        public void LogConfiguracion(string accion, int configuracionId)
        {
            EscribirLog($"CONFIGURACION - {accion} configuración ID: {configuracionId}");
        }

        // Metodo para leer los logs del sistema
        // Lo uso cuando el admin quiere ver que ha pasado en el sistema
        public async Task<IEnumerable<string>> LeerLogsAsync()
        {
            try
            {
                if (!File.Exists(_logPath))
                    return new List<string>();

                var lineas = await File.ReadAllLinesAsync(_logPath);
                return lineas.Reverse().Take(100); // Devuelvo los ultimos 100 logs
            }
            catch (Exception ex)
            {
                // Si falla leer el archivo, uso Console como respaldo
                Console.WriteLine($"Error leyendo logs: {ex.Message}");
                return new List<string> { "Error leyendo logs del sistema" };
            }
        }

        // Metodo privado que escribe en el archivo de log
        // Este es el metodo que realmente guarda la informacion
        private void EscribirLog(string mensaje)
        {
            try
            {
                // Formato del log: [timestamp] mensaje
                var linea = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {mensaje}";
                File.AppendAllText(_logPath, linea + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Si falla escribir en el archivo, uso Console como respaldo
                // Esto es importante para no perder informacion
                Console.WriteLine($"Error escribiendo log: {ex.Message}");
                Console.WriteLine($"Log original: {mensaje}");
            }
        }
    }
}
