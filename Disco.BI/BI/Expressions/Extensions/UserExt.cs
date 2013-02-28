using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;
using Disco.BI.Extensions;
using Disco.BI.Interop.ActiveDirectory;

namespace Disco.BI.Expressions.Extensions
{
    public static class UserExt
    {
        public static object GetActiveDirectoryObjectValue(User User, string PropertyName, int Index = 0)
        {
            var adUserAccount = User.ActiveDirectoryAccount(PropertyName);
            if (adUserAccount != null)
                return adUserAccount.GetPropertyValue(PropertyName, Index);
            else
                return null;
        }

        public static string GetActiveDirectoryStringValue(User User, string PropertyName, int Index = 0)
        {
            var objectValue = GetActiveDirectoryObjectValue(User, PropertyName, Index);
            string stringValue = objectValue as string;
            if (stringValue == null && objectValue != null)
                stringValue = objectValue.ToString();
            return stringValue;
        }

        public static int GetActiveDirectoryIntegerValue(User User, string PropertyName, int Index = 0)
        {
            var objectValue = GetActiveDirectoryObjectValue(User, PropertyName, Index);
            if (objectValue == null)
                return default(int);
            else
            {
                int intValue;
                try
                {
                    intValue = (int)Convert.ChangeType(objectValue, typeof(int));
                }
                catch (Exception)
                {
                    throw;
                }
                return intValue;
            }
        }
    }
}
