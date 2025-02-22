using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Yazlab2.Services
{
    public class EmailService
    {
        private readonly string _smtpServer = "smtp.gmail.com"; // Gmail SMTP sunucu adresi
        private readonly int _smtpPort = 587; // TLS için kullanılan port
        private readonly string _senderEmail = "zeynep.durak7525@gmail.com";
        private readonly string _senderPassword = "qbenmb jjpm jrep senxpd!"; // Google uygulama şifrenizi kullanın

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(_senderEmail),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
            mail.To.Add(email);

            using (var smtp = new SmtpClient(_smtpServer, _smtpPort))
            {
                smtp.Credentials = new NetworkCredential(_senderEmail, _senderPassword);
                smtp.EnableSsl = true;
                try
                {
                    await smtp.SendMailAsync(mail);
                }
                catch (SmtpException ex)
                {
                    // Hata mesajlarını kullanıcıya iletmek için ViewData kullanabilirsiniz
                    Debug.WriteLine(ex);
                }
            }
        }

    }
}
