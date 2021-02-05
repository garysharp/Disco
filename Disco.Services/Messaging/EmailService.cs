using Disco.Data.Configuration;
using Disco.Data.Repository;
using Disco.Models.Services.Messaging;
using System;
using System.Net;
using System.Net.Mail;

namespace Disco.Services.Messaging
{
    public static class EmailService
    {
        private static string smtpServer;
        private static int smtpPort;
        private static string smtpFromAddress;
        private static bool smtpEnableSsl;
        private static string smtpUsername;
        private static string smtpPassword;
        private static string smtpReplyToAddress;

        public static bool IsConfigured { get; private set; }

        static EmailService()
        {
            using (var database = new DiscoDataContext())
            {
                Update(database.DiscoConfiguration);
            }
        }

        public static void ValidateConfiguration(string smtpServer, int smtpPort, string fromAddress, string replyToAddress, bool enableSsl, string username, string password)
        {
            // if smtpServer is null, we aren't configured (emailing is disabled)
            if (!string.IsNullOrWhiteSpace(smtpServer))
            {
                // validate
                if (smtpPort <= 0)
                    throw new ArgumentOutOfRangeException(nameof(smtpPort), "Invalid SMTP port specified");
                if (string.IsNullOrWhiteSpace(fromAddress))
                    throw new ArgumentOutOfRangeException(nameof(fromAddress), "From Address is required");
                // try parse FromAddress
                new MailAddress(fromAddress);
                // try parse reply-to address
                if (!string.IsNullOrWhiteSpace(replyToAddress))
                    new MailAddress(replyToAddress);
            }
        }

        public static void Update(SystemConfiguration systemConfiguration)
        {
            smtpServer = systemConfiguration.EmailSmtpServer;
            smtpPort = systemConfiguration.EmailSmtpPort;
            smtpEnableSsl = systemConfiguration.EmailEnableSsl;
            smtpFromAddress = systemConfiguration.EmailFromAddress;
            smtpUsername = systemConfiguration.EmailUsername;
            smtpPassword = systemConfiguration.EmailPassword;
            smtpReplyToAddress = systemConfiguration.EmailReplyToAddress;

            IsConfigured =
                !string.IsNullOrWhiteSpace(smtpServer) &&
                smtpPort > 0 &&
                !string.IsNullOrWhiteSpace(smtpFromAddress);
        }

        public static void SendEmail(Email email)
        {
            if (!IsConfigured)
                throw new InvalidOperationException("Unable to send email, the email service has not been configured.");

            var message = new MailMessage()
            {
                Subject = email.Subject,
                IsBodyHtml = email.IsBodyHtml,
                Body = email.Body,
            };

            if (!string.IsNullOrWhiteSpace(email.From))
                message.From = new MailAddress(email.From);
            else
                message.From = new MailAddress(smtpFromAddress);

            if (!string.IsNullOrWhiteSpace(email.ReplyTo))
                message.ReplyToList.Add(new MailAddress(email.ReplyTo));
            else if (!string.IsNullOrWhiteSpace(smtpReplyToAddress))
                message.ReplyToList.Add(new MailAddress(smtpReplyToAddress));

            if (email.To.Count > 0)
            {
                foreach (var recipient in email.To)
                    message.To.Add(recipient);
            }
            if (email.CC.Count > 0)
            {
                foreach (var recipient in email.CC)
                    message.CC.Add(recipient);
            }
            if (email.BCC.Count > 0)
            {
                foreach (var recipient in email.BCC)
                    message.Bcc.Add(recipient);
            }

            using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
            {
                smtpClient.EnableSsl = smtpEnableSsl;
                if (!string.IsNullOrWhiteSpace(smtpUsername))
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                smtpClient.Send(message);
            }
        }

        public static void SendTestEmail(string recipient)
        {
            var email = new Email(recipient, "Disco ICT Test Email", @"Disco ICT has successfully been configured to send to this recipient.");
            SendEmail(email);
        }

    }
}
