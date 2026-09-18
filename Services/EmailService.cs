using System.Net;
using System.Net.Mail;

namespace LojaWeb_2.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarEmailAsync(
            string destinatario,
            string assunto,
            string mensagem)
        {
            var host = _configuration["EmailSettings:Host"];
            var porta = int.Parse(
                _configuration["EmailSettings:Port"] ?? "587");

            var email = _configuration["EmailSettings:Email"];
            var senha = _configuration["EmailSettings:Senha"];

            using var smtp = new SmtpClient(host, porta);

            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(email, senha);

            using var mail = new MailMessage();

            mail.From = new MailAddress(
                email!,
                "VesteArte"
            );

            mail.Subject = assunto;
            mail.Body = mensagem;
            mail.IsBodyHtml = true;

            mail.To.Add(destinatario);

            await smtp.SendMailAsync(mail);
        }
    }
}