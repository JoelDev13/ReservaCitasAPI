using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;

namespace Infrastructure.Servicios
{
    // Este es mi EmailService que esta completamente desacoplado de la logica de reservas
    // Lo hice asi porque es un requerimiento de la evaluacion
    // Si el email falla, no debe interrumpir el flujo de reserva
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogService _logService;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _logService = LogService.Instance;
        }

        // Este metodo envia email de confirmacion cuando se reserva una cita
        // Lo hice asi porque quiero que el usuario reciba confirmacion de su reserva
        // Pero si falla, no debe afectar la reserva
        public async Task EnviarConfirmacionCitaAsync(string destinatario, string nombreUsuario, DateTime fechaHora, int estacion, string estado)
        {
            try
            {
                var cliente = CrearClienteSmtp();
                var mail = new MailMessage();

                // Uso la configuracion del appsettings o un valor por defecto
                mail.From = new MailAddress(_configuration["Email:FromAddress"] ?? "noreply@citas.com");
                mail.To.Add(destinatario);
                mail.Subject = "Confirmación de la Cita";
                
                // Cuerpo del email con toda la informacion de la cita
                mail.Body = $@"
                    Estimado/a {nombreUsuario},

                    Su cita ha sido confirmada con los siguientes detalles:
                    
                    Fecha: {fechaHora:dd/MM/yyyy}
                    Hora: {fechaHora:HH:mm}
                    Estación: {estacion}
                    Estado: {estado}

                    Por favor, llegue 10 minutos antes de su cita.
                    
                    Saludos,
                    Sistema de Reserva de Citas
                ";
                mail.IsBodyHtml = false;

                // Envio el email
                await cliente.SendMailAsync(mail);
                
                // Registro en el log que se envio el email
                _logService.LogReserva(0, fechaHora, estacion);
                
                // Log adicional para confirmar que se envio el email
                _logService.LogConfiguracion($"Email de confirmación enviado a {destinatario}", 0);
            }
            catch (Exception ex)
            {
                // Si falla el email, lo registro en el log pero NO lanzo excepcion
                // Esto es importante para mantener el servicio desacoplado
                _logService.LogError($"Error enviando email de confirmacion a {destinatario}", ex);
                // No lanzo excepcion para no interrumpir el flujo de reserva
            }
        }

        // Este metodo envia email de cancelacion cuando se cancela una cita
        // Lo hice asi porque el usuario debe saber que su cita fue cancelada
        // Pero si falla, no debe afectar la cancelacion
        public async Task EnviarCancelacionCitaAsync(string destinatario, string nombreUsuario, DateTime fechaHora)
        {
            try
            {
                var cliente = CrearClienteSmtp();
                var mail = new MailMessage();

                mail.From = new MailAddress(_configuration["Email:FromAddress"] ?? "noreply@citas.com");
                mail.To.Add(destinatario);
                mail.Subject = "Cancelación de Cita";
                
                // Cuerpo del email de cancelacion
                mail.Body = $@"
                    Estimado/a {nombreUsuario},

                    Su cita ha sido CANCELADA:
                    
                    Fecha: {fechaHora:dd/MM/yyyy}
                    Hora: {fechaHora:HH:mm}
                    Estado: CANCELADA

                    Puede reservar una nueva cita cuando usted quiera.
                    
                    Saludos,
                    Sistema de Reserva de Citas
                ";

                await cliente.SendMailAsync(mail);
                
                // Registro en el log que se envio el email de cancelacion
                _logService.LogConfiguracion($"Email de cancelación enviado a {destinatario}", 0);
            }
            catch (Exception ex)
            {
                // Si falla el email, lo registro en el log pero NO lanzo excepcion
                _logService.LogError($"Error enviando email de cancelación a {destinatario}", ex);
                // No lanzo excepcion para no interrumpir el flujo de cancelacion
            }
        }

        // Este metodo privado crea el cliente SMTP para enviar emails
        // Lo hice asi porque quiero centralizar la configuracion del cliente SMTP
        // Y tener fallbacks por si no hay configuracion
        private SmtpClient CrearClienteSmtp()
        {
            var client = new SmtpClient();
            
            // Uso configuracion del appsettings o valores por defecto
            // Esto hace que el sistema sea flexible
            client.Host = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            client.Port = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            
            // Obtengo las credenciales de la configuracion
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            
            // Solo configuro credenciales si estan disponibles
            // Si no hay credenciales, el sistema funciona pero no envia emails
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                client.Credentials = new NetworkCredential(username, password);
            }
            
            return client;
        }
    }
}

