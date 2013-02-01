using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.ClientServices;
using System.Web;
using Disco.Data.Repository;
using Disco.Models.Repository;

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

            using (DiscoDataContext dbContext = new DiscoDataContext())
            {
                EnrolResponse response = DeviceBI.DeviceEnrol.Enrol(dbContext, username, request);
                dbContext.SaveChanges();
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

            using (DiscoDataContext dbContext = new DiscoDataContext())
            {
                User user = UserBI.UserCache.GetUser(username, dbContext, true);
                WhoAmIResponse response = new WhoAmIResponse()
                {
                    Username = user.Id,
                    DisplayName = user.DisplayName,
                    Type = user.Type
                };
                return response;
            }
        }

        public static MacEnrolResponse BuildResponse(this MacEnrol request)
        {
            if (HttpContext.Current == null)
                throw new PlatformNotSupportedException("This function can only be accessed from within ASP.NET");

            using (DiscoDataContext dbContext = new DiscoDataContext())
            {
                MacEnrolResponse response = DeviceBI.DeviceEnrol.MacEnrol(dbContext, request, false);
                dbContext.SaveChanges();
                return response;
            }
        }

    }
}
