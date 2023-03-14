using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Dev.Codes.Lib.EmailService.Interfaces;
using Dev.Codes.Lib.EmailService.Models;
using Microsoft.Extensions.Configuration;

namespace Dev.Codes.Lib.EmailService.EmailSenders
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfiguration;
        
        public SmtpEmailSender(IConfiguration configuration)
        {
            _emailConfiguration = configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
        }
        
        public async Task<bool> SendEmailAsync(Message message)
        {
            var mailMessage = CreateMailMessage(message);
            try
            {
                await SendAsync(mailMessage);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private MailMessage CreateMailMessage(Message message)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(_emailConfiguration.From);
            mailMessage.Subject = message.Subject;
            mailMessage.To.Add(new MailAddress(message.To[0]));
            mailMessage.Body = message.Content;
            mailMessage.IsBodyHtml = message.IsHtmlContent;
            foreach (var attachment in message.filePaths)
            {
                mailMessage.Attachments.Add(new Attachment(attachment));
            }
            return mailMessage;
        }

        private async Task SendAsync(MailMessage message)
        {
            using var client = new SmtpClient(_emailConfiguration.Server)
            {
                Port = _emailConfiguration.Port,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailConfiguration.UserName, _emailConfiguration.Password),
                EnableSsl = true
            };

            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
