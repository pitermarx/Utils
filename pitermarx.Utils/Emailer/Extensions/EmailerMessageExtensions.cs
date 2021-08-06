using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;

namespace pitermarx.Emailer.Extensions
{
    public static class EmailerMessageExtensions
    {
        public static EmailerMessage AddPdfAttachment(this EmailerMessage @this, Stream content, string fileName)
        {
            @this.AddAttachment(content, fileName, "pdf", MediaTypeNames.Application.Pdf);
            return @this;
        }

        public static EmailerMessage AddZipAttachment(this EmailerMessage @this, Stream content, string fileName)
        {
            @this.AddAttachment(content, fileName, "zip", MediaTypeNames.Application.Zip);
            return @this;
        }

        public static string MessagePlainText(this EmailerMessage @this, List<string> attachments)
        {
            const string format = "From: {0}\n" + "To: {1}\n" + "CC: {2}" + "Subject: {3}\n" + "Body: {4}" + "{5}{6}";
            var hasAttacmentst = attachments != null && attachments.Any();
            return string.Format(format,
                @this.MailMessage.From.Address,
                string.Join(", ", @this.MailMessage.To.Select(t => t.Address)),
                string.Join(", ", @this.MailMessage.CC.Select(t => t.Address)),
                @this.MailMessage.Subject,
                @this.MailMessage.Body,
                hasAttacmentst ? "\nAttachments: " : string.Empty,
                hasAttacmentst ? string.Join(", ", attachments) : string.Empty);
        }
    }
}