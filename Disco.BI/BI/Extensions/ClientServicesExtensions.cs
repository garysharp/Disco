using Disco.Data.Repository;
using Disco.Models.ClientServices;
using Disco.Services.Authorization;
using Disco.Services.Users;
using System;
using System.Web;

namespace Disco.BI.Extensions
{
    public static class ClientServicesExtensions
    {
        public static EnrolResponse BuildResponse(this Enrol request)
        {
            if (HttpContext.Current == null)
                throw new PlatformNotSupportedException("This function can only be accessed from within ASP.NET");

            string username = null;
            if (HttpContext.Current.Request.IsAuthenticated)
                username = HttpContext.Current.User.Identity.Name;

            using (DiscoDataContext database = new DiscoDataContext())
            {
                EnrolResponse response = DeviceBI.DeviceEnrol.Enrol(database, username, request);
                database.SaveChanges();
                return response;
            }
        }

        public static WhoAmIResponse BuildResponse(this WhoAmI request)
        {
            if (HttpContext.Current == null)
                throw new PlatformNotSupportedException("This function can only be accessed from within ASP.NET");

            string username = null;
            if (HttpContext.Current.Request.IsAuthenticated)
                username = HttpContext.Current.User.Identity.Name;

            if (username == null)
                throw new InvalidOperationException("Unauthenticated Http Context");

            using (DiscoDataContext database = new DiscoDataContext())
            {
                AuthorizationToken token = UserService.GetAuthorization(username, database, true);

                WhoAmIResponse response = new WhoAmIResponse()
                {
                    Username = token.User.UserId,
                    DisplayName = token.User.DisplayName,
                    Type = token.Has(Claims.ComputerAccount) ? "Computer Account" : "User Account"
                };
                return response;
            }
        }

        public static MacEnrolResponse BuildResponse(this MacEnrol request)
        {
            if (HttpContext.Current == null)
                throw new PlatformNotSupportedException("This function can only be accessed from within ASP.NET");

            using (DiscoDataContext database = new DiscoDataContext())
            {
                MacEnrolResponse response = DeviceBI.DeviceEnrol.MacEnrol(database, request, false);
                database.SaveChanges();
                return response;
            }
        }

        public static RegisterResponse BuildResponse(this Register request)
        {
            if (HttpContext.Current == null)
                throw new PlatformNotSupportedException("This function can only be accessed from within ASP.NET");

            string username = null;
            if (HttpContext.Current.Request.IsAuthenticated)
                username = HttpContext.Current.User.Identity.Name;

            using (DiscoDataContext database = new DiscoDataContext())
            {
                RegisterResponse response = DeviceBI.DeviceEnrol.Register(database, username, request);
                database.SaveChanges();
                return response;
            }
        }

    }
}
