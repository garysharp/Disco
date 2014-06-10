using Disco.Models.Repository;
using Disco.Models.Services.Authorization;
using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.UI.User;
using Disco.Services.Users.UserFlags;
using Disco.Web.Extensions;
using Disco.Web.Models.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Models.User
{
    public class ShowModel : UserShowModel
    {
        public Disco.Models.Repository.User User { get; set; }
        public JobTableModel Jobs { get; set; }
        public List<DocumentTemplate> DocumentTemplates { get; set; }

        public List<UserFlag> AvailableUserFlags { get; set; }

        public IAuthorizationToken AuthorizationToken { get; set; }
        public IClaimNavigatorItem ClaimNavigator { get; set; }

        public FancyTreeNode[] ClaimNavigatorFancyTreeNodes
        {
            get
            {
                var rootNode = FancyTreeNode.FromClaimNavigatorItem(this.ClaimNavigator, true);
                rootNode.expanded = true;

                return new FancyTreeNode[] {
                    rootNode
                };
            }
        }

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