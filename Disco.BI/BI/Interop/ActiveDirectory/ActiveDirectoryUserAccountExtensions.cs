using Disco.Models.Interop.ActiveDirectory;
using System;
using Disco.Models.Repository;
namespace Disco.BI.Interop.ActiveDirectory
{
    internal static class ActiveDirectoryUserAccountExtensions
    {
        public static bool HasRole(this ActiveDirectoryUserAccount account, string Role)
        {
            return account.Groups != null && account.Groups.Contains(Role.ToLower());
        }

        public static object GetPropertyValue(this ActiveDirectoryUserAccount account, string PropertyName, int Index = 0)
        {
            switch (PropertyName.ToLower())
            {
                case "name":
                    return account.Name;
                case "samaccountname":
                    return account.sAMAccountName;
                case "distinguishedname":
                    return account.DistinguishedName;
                case "objectsid":
                    return account.ObjectSid;
                case "sn":
                    return account.Surname;
                case "givenname":
                    return account.GivenName;
                case "mail":
                    return account.Email;
                case "telephonenumber":
                    return account.Phone;
                default:
                    object[] adProperty;
                    if (account.LoadedProperties.TryGetValue(PropertyName, out adProperty) && Index <= adProperty.Length)
                        return adProperty[Index];
                    else
                        return null;
            }
        }
    }
}
