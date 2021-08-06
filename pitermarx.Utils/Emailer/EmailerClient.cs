using System.Net;
using System.Net.Mail;

namespace pitermarx.Emailer
{
    public class EmailerClient
    {
        public SmtpClient SmtpClient { get; private set; }

        private EmailerClient() { }

        public EmailerClient At(string host, int port, int timeout = 60000)
        {
            this.SmtpClient = new SmtpClient(host, port)
            {
                // default timeout 1min
                Timeout = timeout
            };

            return this;
        }

        public static EmailerClient Create()
        {
            return new EmailerClient();
        }

        public EmailerClient WithCredentials(string user, string password, bool useSsl = true)
        {
            this.SmtpClient.UseDefaultCredentials = false;
            this.SmtpClient.Credentials = new NetworkCredential(user, password);
            this.SmtpClient.EnableSsl = useSsl; // auto detect ssl/tls
            return this;
        }

        public void Dispose()
        {
            if (this.SmtpClient != null)
            {
                this.SmtpClient.Dispose();
            }
        }
    }
}