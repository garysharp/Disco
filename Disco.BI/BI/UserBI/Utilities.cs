using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;
using Disco.Data.Repository;
using Disco.Models.BI.Search;
using System.Runtime.InteropServices;
using System.DirectoryServices.ActiveDirectory;
using Disco.Services.Logging;

namespace Disco.BI.UserBI
{
    public static class Utilities
    {

        public static User LoadUser(DiscoDataContext dbContext, string Username)
        {
            // Machine Account ?
            if (Username.EndsWith("$"))
            {
                return Interop.ActiveDirectory.ActiveDirectory.GetMachineAccount(Username).ToRepositoryUser();
            }

            // User Account
            User user = null;
            try
            {
                var ADUser = Interop.ActiveDirectory.ActiveDirectory.GetUserAccount(Username);
                if (ADUser == null)
                    throw new ArgumentException(string.Format("Invalid Username: '{0}'", Username), "Username");
                user = ADUser.ToRepositoryUser();
            }
            catch (COMException ex)
            {
                // If "Server is not operational" then Try Cache
                if (ex.ErrorCode != -2147016646)
                {
                    throw ex;
                }
                SystemLog.LogException("Primary Domain Controller Down? Disco.BI.UserBI.Utilities.LoadUser", ex);
            }
            catch (ActiveDirectoryOperationException ex)
            {
                // Try From Cache...
                SystemLog.LogException("Primary Domain Controller Down? Disco.BI.UserBI.Utilities.LoadUser", ex);
            }

            // Update Repository
            User existingUser;
            if (user == null)
            {
                string username = Username.Contains(@"\") ? Username.Substring(Username.IndexOf(@"\") + 1) : Username;
                existingUser = dbContext.Users.Find(username);
                if (existingUser == null)
                    throw new ArgumentException(string.Format("Invalid User - Not In Disco DB: '{0}'", Username), "Username");
                else
                    return existingUser;
            }
            existingUser = dbContext.Users.Find(user.Id);
            if (existingUser == null)
            {
                dbContext.Users.Add(user);
            }
            else
            {
                existingUser.UpdateSelf(user);
                user = existingUser;
            }
            dbContext.SaveChanges();

            return user;
        }

    }
}
