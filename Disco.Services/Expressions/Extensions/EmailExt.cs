using Disco.Models.Repository;
using Disco.Models.Services.Messaging;
using Disco.Services.Messaging;
using System;

namespace Disco.Services.Expressions.Extensions
{
    public static class EmailExt
    {
        public static void SendToUser(User user, string subject, string body)
        {
            var to = user.EmailAddress;

            if (string.IsNullOrWhiteSpace(to))
                throw new Exception($"User ({user.UserId}) does not have an email address");

            Send(to, subject, body);
        }

        public static void Send(string to, string subject, string body)
        {
            var email = new Email()
            {
                Subject = subject,
                Body = body,
            };
            email.To.Add(to);

            EmailService.SendEmail(email);
        }
    }
}
