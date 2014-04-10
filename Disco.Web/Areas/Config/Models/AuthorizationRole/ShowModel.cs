using Disco.Models.Services.Authorization;
using Disco.Models.Interop.ActiveDirectory;
using Disco.Models.UI.Config.AuthorizationRole;
using Disco.Web.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.Config.Models.AuthorizationRole
{
    public class ShowModel : ConfigAuthorizationRoleShowModel
    {
        public IRoleToken Token { get; set; }

        public List<SubjectDescriptor> Subjects { get; set; }

        public IClaimNavigatorItem ClaimNavigator { get; set; }

        public FancyTreeNode[] ClaimNavigatorFancyTreeNodes
        {
            get
            {
                var rootNode = FancyTreeNode.FromClaimNavigatorItem(this.ClaimNavigator, false);
                rootNode.expanded = true;

                return new FancyTreeNode[] {
                    rootNode
                };
            }
        }

        public class SubjectDescriptor
        {
            public bool IsGroup { get; set; }
            public string Name { get; set; }
            public string Id { get; set; }

            public static SubjectDescriptor FromActiveDirectoryObject(IActiveDirectoryObject ADObject)
            {
                var item = new SubjectDescriptor()
                {
                    Id = ADObject.NetBiosId,
                    Name = ADObject.Name
                };

                if (ADObject is ActiveDirectoryGroup)
                    item.IsGroup = true;

                return item;
            }
        }
    }
}