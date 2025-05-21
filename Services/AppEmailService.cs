using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace GuidanceTracker.Services
{
    public class AppEmailService
    {
        // to send emails without using the inbuilt email service
        public Task SendAsync(string to, string subject, string bodyHtml)
        {
            return Task.Run(() =>
            {
                string smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
                string smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS");

                if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                    throw new Exception("Missing SMTP credentials.");

                MailMessage mail = new MailMessage
                {
                    From = new MailAddress(smtpUser),
                    Subject = subject,
                    Body = bodyHtml,
                    IsBodyHtml = true
                };

                mail.To.Add(to);

                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    client.Send(mail);
                }
            });
        }
    }
}
