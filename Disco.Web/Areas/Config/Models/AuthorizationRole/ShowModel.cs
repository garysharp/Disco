using Disco.Models.Services.Authorization;
using Disco.Models.UI.Config.AuthorizationRole;
using Disco.Web.Areas.API.Models.Shared;
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

        public List<SubjectDescriptorModel> Subjects { get; set; }

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
    }
}