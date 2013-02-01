using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.BI;
using Disco.BI.Extensions;
using Disco.Models.Interop.ActiveDirectory;
using Disco.Web.Extensions;

namespace Disco.Web.Models.User
{
    public class ShowModel
    {
        public Disco.Models.Repository.User User { get; set; }
        public Disco.Models.BI.Job.JobTableModel Jobs { get; set; }
        public List<Disco.Models.Repository.DocumentTemplate> DocumentTemplates { get; set; }

        public List<SelectListItem> DocumentTemplatesSelectListItems
        {
            get
            {
                var list = new List<SelectListItem>();
                list.Add(new SelectListItem() { Selected = true, Value = string.Empty, Text = "Select a Document to Generate" });
                list.AddRange(this.DocumentTemplates.ToSelectListItems());
                return list;
            }
        }

        public string PrimaryDeviceSerialNumber
        {
            get
            {
                var assignedDevice = User.DeviceUserAssignments.Where(d => !d.UnassignedDate.HasValue).FirstOrDefault();
                if (assignedDevice == null)
                {
                    return null;
                }
                else
                {
                    return assignedDevice.DeviceSerialNumber;
                }
            }
        }
    }
}