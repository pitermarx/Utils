using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using pitermarx.Utils;

namespace pitermarx.Emailer
{
    public class EmailerMessage
    {
        private const float BytesPerMegaByte = 1048576f; // == 1024^2

        public MailMessage MailMessage { get; private set; }

        public int? MaxAttachmentSize { get; private set; }

        public bool EmptyToEmails { get; private set; }

        public List<Tuple<string, float>> AttachmentsWithSizeExceeded
        {
            get
            {
                if (MaxAttachmentSize.HasValue)
                {
                    float attchSize = 0;
                    foreach (var attach in MailMessage.Attachments)
                    {
                        attchSize = +attach.ContentStream.Length / BytesPerMegaByte;
                    }
                    // verify if the sizes sum of all attached files exceeds the attachs size limit
                    if (attchSize > this.MaxAttachmentSize.Value)
                    {
                        var message = new List<Tuple<string, float>>
                        {
                            Tuple.Create(MailMessage.Attachments.Count > 1 ? MailMessage.Subject : MailMessage.Attachments.First().Name, attchSize)
                        };
                        return message;
                    }
                }

                return new List<Tuple<string, float>>();
            }
        }

        private EmailerMessage()
        {
            this.MailMessage = new MailMessage();
        }

        public static EmailerMessage Create()
        {
            return new EmailerMessage();
        }

        public EmailerMessage WithSubject(string subject)
        {
            this.MailMessage.Subject = subject;
            return this;
        }

        public EmailerMessage WithBody(string body)
        {
            this.MailMessage.Body = body;
            return this;
        }

        public EmailerMessage AddAttachment(Stream content, string fileName, string extension, string mimeType)
        {
            var escapedFileName = GetAttachmentName(fileName, extension);
            var attachment = new Attachment(content, escapedFileName, mimeType);
            this.MailMessage.Attachments.Add(attachment);

            return this;
        }

        private static string GetAttachmentName(string fileName, string extension)
        {
            byte[] bytes = Encoding.Default.GetBytes(fileName);
            fileName = Encoding.ASCII.GetString(bytes);
            if (fileName.Length > 60)
            {
                fileName = fileName.Substring(0, 60);
            }

            return string.Format("{0}.{1}", fileName.AppendTimeStamp(DateTime.UtcNow), extension);
        }

        public EmailerMessage ToEmail(string toEmail, string name = null)
        {
            if (!string.IsNullOrEmpty(toEmail))
            {
                if (string.IsNullOrEmpty(name))
                {
                    this.MailMessage.To.Add(toEmail);
                }
                else
                {
                    this.MailMessage.To.Add(new MailAddress(toEmail, name));
                }
            }
            else
            {
                this.EmptyToEmails = true;
            }

            return this;
        }

        public EmailerMessage ToEmails(string[] toEmail)
        {
            if (toEmail != null)
            {
                toEmail.ToList().ForEach(m => this.ToEmail(m));
            }
            return this;
        }

        public EmailerMessage From(string senderEmail, string name = null)
        {
            if (!string.IsNullOrEmpty(senderEmail))
            {
                this.MailMessage.From =
                    string.IsNullOrEmpty(name)
                        ? new MailAddress(senderEmail)
                        : new MailAddress(senderEmail, name);
            }
            return this;
        }

        public EmailerMessage WithMaxAttachment(int max)
        {
            this.MaxAttachmentSize = max;
            return this;
        }

        public void Dispose()
        {
            if (this.MailMessage != null)
            {
                this.MailMessage.Dispose();
            }
        }
    }
}