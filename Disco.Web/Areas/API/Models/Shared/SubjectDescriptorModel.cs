using Disco.Services.Interop.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.API.Models.Shared
{
    public class SubjectDescriptorModel
    {
        public string Id { get; set; }

        public string Name { get; set; }
        public string Type { get; set; }

        public bool IsGroup { get; set; }
        public bool IsUserAccount { get; set; }
        public bool IsMachineAccount { get; set; }

        public static SubjectDescriptorModel FromActiveDirectoryObject(IADObject ADObject)
        {
            var item = new SubjectDescriptorModel()
            {
                Id = ADObject.Id,
                Name = ADObject.DisplayName
            };

            if (ADObject is ADUserAccount)
            {
                item.IsUserAccount = true;
                item.Type = "user";
            }
            else if (ADObject is ADGroup)
            {
                item.IsGroup = true;
                item.Type = "group";
            }
            else if (ADObject is ADMachineAccount)
            {
                item.IsMachineAccount = true;
                item.Type = "machine";
            }
            else
            {
                item.Type = "unknown";
            }

            return item;
        }
    }
}