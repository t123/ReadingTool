using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public class EmailService : IEmailService
    {
        private log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private void SendEmail(User user, string subject, string body)
        {
            try
            {
                SmtpClient client = new SmtpClient();
                MailMessage message = new MailMessage("info@readingtool.net", user.EmailAddress, subject, body);
                client.Send(message);
            }
            catch(Exception e)
            {
                _logger.Error(e);
            }
        }

        public void ResetPasswordInstructions(User user)
        {
            if(user == null || string.IsNullOrEmpty(user.EmailAddress))
            {
                return;
            }

            string email = string.Format(@"Hi there {0},

If you requested a new password, please click the link below within the next hour:
http://readingtool.net/reset-password/{1}?key={2}

If you cannot click on the link please copy and paste it into your browsers address bar.
If you did not request a new password please ignore this email.

Thanks, 
readingtool.net
", user.Username, user.Username, HttpUtility.UrlEncode(user.ForgotPasswordRequest.First().ResetKey));

            SendEmail(user, "Forgotten password request", email);
        }

        public void ResetSuccess(User user)
        {
            if(user == null || string.IsNullOrEmpty(user.EmailAddress))
            {
                return;
            }

            string email = string.Format(@"Hi there {0},

Just a note that your password has been reset.

Thanks, 
readingtool.net
", user.Username);

            SendEmail(user, "Password reset", email);
        }
    }
}
