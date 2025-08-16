namespace Application.Interfaces
{
    public interface IEmailService
    {
        Task EnviarConfirmacionCitaAsync(string destinatario, string nombreUsuario, DateTime fechaHora, int estacion, string estado);
        Task EnviarCancelacionCitaAsync(string destinatario, string nombreUsuario, DateTime fechaHora);
    }
}
