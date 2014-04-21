using Disco.Services.Interop.ActiveDirectory;
using Disco.Web.Models.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Web.Areas.API.Models.System
{
    public class DomainOrganisationalUnitsModel
    {
        public ADDomain Domain { get; set; }
        public List<ADOrganisationalUnit> OrganisationalUnits { get; set; }

        public FancyTreeNode ToFancyTreeNode()
        {
            FancyTreeNode[] children = OrganisationalUnits.Select(ou => OrganisationalUnitToFancyTreeNode(ou)).ToArray();

            return new FancyTreeNode()
            {
                key = Domain.DistinguishedName,
                title = Domain.NetBiosName,
                folder = true,
                tooltip = Domain.Name,
                children = children,
                unselectable = false,
                expanded = true
            };
        }
        private FancyTreeNode OrganisationalUnitToFancyTreeNode(ADOrganisationalUnit OrganisationalUnit)
        {
            FancyTreeNode[] children = OrganisationalUnit.Children == null
                ? null
                : OrganisationalUnit.Children.Select(ou => OrganisationalUnitToFancyTreeNode(ou)).ToArray();

            return new FancyTreeNode()
            {
                key = OrganisationalUnit.DistinguishedName,
                title = OrganisationalUnit.Name,
                folder = true,
                tooltip = OrganisationalUnit.DistinguishedName,
                children = children,
                unselectable = false
            };
        }
    }
}