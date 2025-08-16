using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;

namespace Infrastructure.Servicios
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogService _logService;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _logService = LogService.Instance;
        }

        public async Task EnviarConfirmacionCitaAsync(string destinatario, string nombreUsuario, DateTime fechaHora, int estacion, string estado)
        {
            try
            {
                var cliente = CrearClienteSmtp();
                var mail = new MailMessage();

                mail.From = new MailAddress(_configuration["Email:FromAddress"]);
                mail.To.Add(destinatario);
                mail.Subject = "Confirmación de la Cita";
                mail.Body = $@"
                    Estimado/a {nombreUsuario},

                    Su cita ha sido confirmada con los siguientes detalles:
                    
                    Fecha: {fechaHora:dd/MM/yyyy}
                    Hora: {fechaHora:HH:mm}
                    Estación: {estacion}
                    Estado: {estado}

                    Por favor, llegue 10 minutos antes de su cita.
                ";
                mail.IsBodyHtml = false;

                await cliente.SendMailAsync(mail);
                _logService.LogReserva(0, fechaHora, estacion);
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error enviando email de confirmacion a {destinatario}", ex);
                
            }
        }

        public async Task EnviarCancelacionCitaAsync(string destinatario, string nombreUsuario, DateTime fechaHora)
        {
            try
            {
                var cliente = CrearClienteSmtp();
                var mail = new MailMessage();

                mail.From = new MailAddress(_configuration["Email:FromAddress"]);
                mail.To.Add(destinatario);
                mail.Subject = "Cancelación de Cita";
                mail.Body = $@"
                    Estimado/a {nombreUsuario},

                    Su cita ha sido CANCELADA:
                    
                    Fecha: {fechaHora:dd/MM/yyyy}
                    Hora: {fechaHora:HH:mm}
                    Estado: CANCELADA

                    Puede reservar una nueva cita cuando usted quiera.
                ";

                await cliente.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error enviando email de cancelación a {destinatario}", ex);
            }
        }

        private SmtpClient CrearClienteSmtp()
        {
            var client = new SmtpClient();
            client.Host = _configuration["Email:SmtpHost"];
            client.Port = int.Parse(_configuration["Email:SmtpPort"]);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(
                _configuration["Email:Username"],
                _configuration["Email:Password"]);
            return client;
        }
    }
}

