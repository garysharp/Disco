using Disco.Models.Interop.ActiveDirectory;
using System;
using Disco.Models.Repository;
namespace Disco.BI.Interop.ActiveDirectory
{
    internal static class ActiveDirectoryUserAccountExtensions
    {
        public static object GetPropertyValue(this ActiveDirectoryUserAccount account, string PropertyName, int Index = 0)
        {
            switch (PropertyName.ToLower())
            {
                case "name":
                    return account.Name;
                case "samaccountname":
                    return account.SamAccountName;
                case "distinguishedname":
                    return account.DistinguishedName;
                case "objectsid":
                    return account.SecurityIdentifier;
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
