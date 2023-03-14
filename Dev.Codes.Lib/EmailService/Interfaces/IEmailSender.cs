using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev.Codes.Lib.EmailService.Models;

namespace Dev.Codes.Lib.EmailService.Interfaces
{
    public interface IEmailSender
    {
        Task<bool> SendEmailAsync(Message message);
    }
}
