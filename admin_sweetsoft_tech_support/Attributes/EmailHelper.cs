using System.Net.Mail;
using System.Net;

namespace admin_sweetsoft_tech_support.Attributes
{
    public class EmailHelper
    {
        private readonly IConfiguration _configuration;

        public EmailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // Lấy thông tin từ cấu hình
            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpServer = emailSettings["SmtpServer"];
            var port = int.Parse(emailSettings["Port"]);
            var fromEmail = emailSettings["FromEmail"];
            var password = emailSettings["Password"];

            // Cấu hình SMTP client
            using var client = new SmtpClient(smtpServer)
            {
                Port = port,
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true,
            };

            // Tạo email
            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, "Support Team"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            // Gửi email
            await client.SendMailAsync(mailMessage);
        }
    }
}
