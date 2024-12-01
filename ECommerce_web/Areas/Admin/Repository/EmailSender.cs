using System.Net.Mail;
using System.Net;

namespace ECommerce_web.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587) //cổng 587
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("tragiang080404@gmail.com", "gnnsfstcnacadzgf") //Để theo mail của mình, đặt mật khẩu ứng dụng
            };

            return client.SendMailAsync(
                new MailMessage(from: "tragiang080404@gmail.com",
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}
