using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
    public class EmailSender : IEmailSender
    {
        public string SendGridSecret { get; set; }
        //IConfiguration _conf có thể vào program khai báo giống triped hoặc làm như vậy?
        public EmailSender(IConfiguration _config)
        {
            SendGridSecret = _config.GetValue<string>("SendGrid:SecretKey"); //lấy khóa bí mật trong appsetting.json
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // xử lý logic email

            //khởi tạo đối tượng SendGridClient với khóa bí mật SendGridSecret
            //var client = new SendGridClient(SendGridSecret);

            //var from = new EmailAddress("ten_email_người_gửi","noi_dung");

            //var to = new EmailAddress(email);

            //var message=MailHelper.CreateSingleEmail(from, to, subject,"", htmlMessage);

            //return client.SendEmailAsync(message);
            
            return Task.CompletedTask;
        }
    }
}
