using System.Collections.Generic;

namespace Disco.Models.Services.Messaging
{
    public class Email
    {
        public string From { get; set; }
        public string ReplyTo { get; set; }
        public List<string> To { get; } = new List<string>();
        public List<string> CC { get; } = new List<string>();
        public List<string> BCC { get; } = new List<string>();

        public string Subject { get; set; }
        public bool IsBodyHtml { get; set; }
        public string Body { get; set; }

        public List<EmailAttachment> Attachments { get; } = new List<EmailAttachment>();

        public Email()
        {
        }

        public Email(string to, string subject, string body)
            : this()
        {
            To.Add(to);
            Subject = subject;
            Body = body;
        }
    }
}
