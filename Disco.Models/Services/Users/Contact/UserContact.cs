using Disco.Models.Repository;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Disco.Models.Services.Users.Contact
{
    public abstract class UserContact
    {
        public User User { get; }
        public UserContactType ContactType { get; }
        public string Source { get; }
        public string Name { get; }

        public abstract string Value { get; }

        public UserContact(User user, UserContactType contactType, string source, string name)
        {
            User = user;
            ContactType = contactType;
            Source = source;
            Name = name;
        }

        protected static bool TryParse<T>(User user, string source, Regex validator, string value, Func<User, string, string, string, T> generator, out T instance) where T : UserContact
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                instance = null;
                return false;
            }

            var match = validator.Match(value);

            if (!match.Success)
            {
                instance = null;
                return false;
            }

            var result =  match.Value;
            var name = default(string);
            if (match.Index > 0)
            {
                name = value.Substring(0, match.Index).Trim();
                if (name.Length > 0)
                {
                    switch (name.Last())
                    {
                        case '<':
                        case '[':
                        case '(':
                        case '{':
                        case '-':
                            name = name.Substring(0, name.Length - 1).Trim();
                            break;
                    }
                }
                if (name.Length == 0)
                    name = default;
            }

            instance = generator(user, source, name, result);
            return true;
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                return $"{Name} <{Value}>";
            }
            else
            {
                return Value;
            }
        }
    }

    public sealed class UserContactEmail : UserContact
    {
        private static Regex validator = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string EmailAddress { get; }
        public override string Value => EmailAddress;

        public UserContactEmail(User user, string source, string name, string emailAddress)
            : base(user, UserContactType.Email, source, name)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                throw new ArgumentNullException(nameof(emailAddress));

            EmailAddress = emailAddress;
        }

        public static bool TryParse(User user, string source, string value, out UserContactEmail contact) =>
            TryParse(user, source, validator, value,
                (u, s, name, emailAddress) => new UserContactEmail(u, s, name, emailAddress),
                out contact);
    }

    public sealed class UserContactAustralianPhone : UserContact
    {
        private static Regex validator = new Regex(@"(?:\+?61\s*[0-9][ \-\.]*?|0[0-9][ \-\.]*?|[(\[]\s*0[0-9]\s*[)\]])\s*(?:[0-9][ \-\.]*?){8}(?=\s*[>\]})\-]?\s*$)", RegexOptions.Compiled);

        public string PhoneNumber { get; }
        public override string Value => PhoneNumber;

        public UserContactAustralianPhone(User user, string source, string name, string phoneNumber)
            : base(user, UserContactType.Phone, source, name)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentNullException(nameof(phoneNumber));

            PhoneNumber = phoneNumber;
        }

        public static bool TryParse(User user, string source, string value, out UserContactAustralianPhone contact) =>
            TryParse(user, source, validator, value,
                (u, s, name, phoneNumber) => new UserContactAustralianPhone(u, s, name, phoneNumber),
                out contact);

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Name))
                return $"{Name} <{PhoneNumber}>";
            else
                return PhoneNumber;
        }
    }

    public sealed class UserContactAustralianMobile : UserContact
    {
        private static Regex validator = new Regex(@"(?:\+?61\s*4[ \-\.]*?|04[ \-\.]*?|[(\[]\s*04\s*[)\]])\s*(?:[0-9][ \-\.]*?){8}(?=\s*[>\]})\-]?\s*$)", RegexOptions.Compiled);

        public string PhoneNumber { get; }
        public override string Value => PhoneNumber;

        public UserContactAustralianMobile(User user, string source, string name, string phoneNumber)
            : base(user, UserContactType.MobilePhone, source, name)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentNullException(nameof(phoneNumber));

            PhoneNumber = phoneNumber;
        }

        public static bool TryParse(User user, string source, string value, out UserContactAustralianPhone contact) =>
            TryParse(user, source, validator, value,
                (u, s, name, phoneNumber) => new UserContactAustralianPhone(u, s, name, phoneNumber),
                out contact);

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Name))
                return $"{Name} <{PhoneNumber}>";
            else
                return PhoneNumber;
        }
    }
}
