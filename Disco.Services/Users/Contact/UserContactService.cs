using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Users.Contact;
using Disco.Services.Plugins.Features.DetailsProvider;
using System.Collections.Generic;
using System.Linq;
using ZXing;

namespace Disco.Services.Users.Contact
{
    public static class UserContactService
    {
        public static List<UserContact> GetContacts(DiscoDataContext database, User user)
            => GetContacts(database, user, null);

        public static List<UserContact> GetContacts(DiscoDataContext database, User user, UserContactType? contactType = null)
        {
            var contacts = new List<UserContact>();

            if (!contactType.HasValue || contactType.Value.HasFlag(UserContactType.Email))
            {
                if (!string.IsNullOrWhiteSpace(user.EmailAddress) &&
                    UserContactEmail.TryParse(user, "Active Directory", $"{user.DisplayName} <{user.EmailAddress}>", out var contact))
                {
                    contacts.Add(contact);
                }
            }

            var foundMobilePhone = false;
            if (!contactType.HasValue || contactType.Value.HasFlag(UserContactType.MobilePhone))
            {
                if (!string.IsNullOrWhiteSpace(user.PhoneNumber) &&
                    UserContactAustralianMobile.TryParse(user, "Active Directory", $"{user.DisplayName} <{user.PhoneNumber}>", out var contact))
                {
                    contacts.Add(contact);
                    foundMobilePhone = true;
                }
            }

            if (!foundMobilePhone && (!contactType.HasValue || contactType.Value.HasFlag(UserContactType.Phone)))
            {
                if (!string.IsNullOrWhiteSpace(user.PhoneNumber) &&
                    UserContactAustralianPhone.TryParse(user, "Active Directory", $"{user.DisplayName} <{user.PhoneNumber}>", out var contact))
                {
                    contacts.Add(contact);
                }
            }

            // from plugin feature
            var features = Plugins.Plugins.GetPluginFeatures(typeof(UserContactFeature));
            foreach (var feature in features)
            {
                var instance = feature.CreateInstance<UserContactFeature>();
                contacts.AddRange(instance.GetContacts(database, user, contactType));
            }

            // from user details
            contacts.AddRange(GetContactsFromUserDetails(database, user, contactType));

            return contacts;
        }

        public static IEnumerable<UserContact> GetContactsFromUserDetails(DiscoDataContext database, User user, UserContactType? contactType = null)
        {
            var service = new DetailsProviderService(database);

            user = database.Users.First(u => u.UserId == user.UserId);
            var details = service.GetDetails(user);

            if ((details?.Count ?? 0) == 0)
                yield break;

            foreach (var item in details)
            {
                if (!contactType.HasValue || contactType.Value.HasFlag(UserContactType.Email))
                {
                    if (UserContactEmail.TryParse(user, item.Key, item.Value, out var contact))
                    {
                        yield return contact;
                        continue;
                    }
                }

                if (!contactType.HasValue || contactType.Value.HasFlag(UserContactType.MobilePhone))
                {
                    if (UserContactAustralianMobile.TryParse(user, item.Key, item.Value, out var contact))
                    {
                        yield return contact;
                        continue;
                    }
                }

                if (!contactType.HasValue || contactType.Value.HasFlag(UserContactType.Phone))
                {
                    if (UserContactAustralianPhone.TryParse(user, item.Key, item.Value, out var contact))
                    {
                        yield return contact;
                        continue;
                    }
                }
            }
        }
    }
}
