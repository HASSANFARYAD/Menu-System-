using MimeKit;


namespace HealthGuage.HelpingClasses
{
    public class MailSender
    {
        private readonly ProjectVariables projectVariables;

        public MailSender(ProjectVariables _projectVariables)
        {
            projectVariables = _projectVariables;
        }

        public async Task<bool> SendForgotPasswordEmailAsync(string email, string id, string BaseUrl = "")
        {
            try
            {
                string MailBody = "<html>" +
                    "<head></head>" +
                    "<body>" +
                    "<center>" + "<div> <h1 class='text-center' style='color:#000000'> Password Reset </h1> " +
                    "<p class='text-center' style='color:#000000'> " +
                          "You are Getting this Email Because You Requested To Reset Your Account Password.<br>Click the Button Below To Change Your Password" +
                    " </p>" +
                    "<p style='color:#000000' class='text-center'>" +
                            "If you did not request a password reset, Please Ignore This Email" +
                    "</p>" +
                    "<h3 style='color:#000000'>" + "Thanks" + "</h3>" +
                    "<br/>" +
                    "<button style='background-color: #CE2029; padding:12px 16px; border:1px solid #CE2029; border-radius:3px;'>" +
                            "<a href='" + BaseUrl + "Auth/ResetPassword?encId=" + id + "&t=" + GeneralPurpose.DateTimeNow().Ticks + "' style='text-decoration:none; font-size:15px; color:white;'> Reset Password </a>" +
                    "</button>" +
                    "<p style='color:#FF0000'>Link will Expire after Date Change.<br>" +
                    "Link will not work in spam. Please move this mail into your inbox.</p>" +
                    "</div>" + "</center>" +
                            "<script src = 'https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js' ></ script ></ body ></ html >";


                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Heather Stock", projectVariables.FromEmail));
                message.To.Add(new MailboxAddress("Heather Stock", email));
                message.Subject = "Heather Stock | Forgot Password";
                message.Body = new TextPart("html")
                {
                    Text = MailBody
                };
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.AuthenticationMechanisms.Remove("NTLM");
                    client.Authenticate(projectVariables.FromEmail, projectVariables.FromEmailPassword);
                    await client.SendAsync(message);
                    client.Disconnect(true);
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
