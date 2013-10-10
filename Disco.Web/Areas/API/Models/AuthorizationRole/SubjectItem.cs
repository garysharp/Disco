using Disco.Models.Interop.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.API.Models.AuthorizationRole
{
    public class SubjectItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public static SubjectItem FromActiveDirectoryObject(IActiveDirectoryObject ADObject)
        {
            return new Models.AuthorizationRole.SubjectItem()
            {
                Id = ADObject.SamAccountName,
                Name = ADObject.Name,
                Type = ADObject is ActiveDirectoryGroup ? "group" : "user"
            };
        }
    }
}