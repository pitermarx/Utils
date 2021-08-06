using System;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace pitermarx.Emailer
{
    public class Emailer
    {
        public ILogger Logger { get; set; }

        public EmailerClient Client { get; private set; }

        public EmailerMessage Message { get; private set; }

        public Action<Exception> ExceptionHandler { get; private set; }

        public bool DoNotRethrow { get; private set; }

        public static Builder Create()
        {
            return new Builder();
        }

        public class Builder
        {
            private readonly Emailer emailer = new();

            public Emailer Emailer => emailer;

            public Builder With(EmailerClient client)
            {
                this.Emailer.Client = client;
                return this;
            }

            public Builder With(EmailerMessage message)
            {
                this.Emailer.Message = message;
                return this;
            }

            public Builder With(ILogger logger)
            {
               this.Emailer.Logger = logger;
               return this;
            }

            public Builder OnException(Action<Exception> exceptionHandler)
            {
                this.Emailer.ExceptionHandler = exceptionHandler;
                return this;
            }

            public Builder DoNotRethrow()
            {
                this.Emailer.DoNotRethrow = true;
                return this;
            }

            private Emailer Build()
            {
                return this.Emailer;
            }

            public bool Send()
            {
                return this.Build().SendEmail();
            }
        }

        private bool SendEmail()
        {
            try
            {
                if (this.Message.MailMessage.To.Count == 0)
                {
                    throw new Exception("No recipients configured");
                }

                var exceededSize = this.Message.AttachmentsWithSizeExceeded;

                if (exceededSize.Any())
                {
                    var file = exceededSize.First();
                    throw new Exception(
                        string.Format(
                            "File {0} exceeds maximum {1} MB for an attachment",
                            file.Item2.ToString("F2", CultureInfo.InvariantCulture),
                            this.Message.MaxAttachmentSize));
                }

                this.Logger?.LogDebug("Sending email: {0}", this.Message.MailMessage.Subject);
                this.Logger?.LogDebug("Recipients: {0}", string.Join(", ", this.Message.MailMessage.To.Select(r => r.Address)));
                this.Logger?.LogDebug(
                   "SMTP configuration: (Host:{0}, Port:{1})",
                   this.Client.SmtpClient.Host,
                   this.Client.SmtpClient.Port);

                this.Client.SmtpClient.Send(this.Message.MailMessage);
                //this.Logger.Info("Email Sent");

                return true;
            }
            catch (Exception e)
            {
                this.HandleException(e);
                return false;
            }
            finally
            {
                this.Dispose();
            }
        }

        private void HandleException(Exception exception)
        {
            this.Logger?.LogError("An error occurred while sending the email", exception);

            try
            {
                this.ExceptionHandler?.Invoke(exception);
            }
            catch (Exception e)
            {
                this.Logger?.LogError("An error occurred when handling the exception", e);
            }

            if (this.DoNotRethrow)
            {
                return;
            }

            var message = exception switch
            {
                // TODO phmarques : resources for the various error codes
                SmtpException e when e.StatusCode == 0 => "Cannot send the email.",
                _ => "Cannot send email",
            };

            throw new Exception(message, exception);
        }

        public void Dispose()
        {
            Client?.Dispose();
            Message?.Dispose();
        }
    }
}