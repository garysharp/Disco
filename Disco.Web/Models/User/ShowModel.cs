using Disco.Models.Repository;
using Disco.Models.Services.Authorization;
using Disco.Models.Services.Documents;
using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.Services.Plugins.Details;
using Disco.Models.UI.User;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.DocumentHandlerProvider;
using Disco.Web.Models.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Web.Models.User
{
    public class ShowModel : UserShowModel
    {
        public Disco.Models.Repository.User User { get; set; }
        public JobTableModel Jobs { get; set; }
        public List<DocumentTemplate> DocumentTemplates { get; set; }
        public List<DocumentTemplatePackage> DocumentTemplatePackages { get; set; }
        public GenerateDocumentControlModel GenerateDocumentControlModel => new GenerateDocumentControlModel()
        {
            Target = User,
            Templates = DocumentTemplates,
            TemplatePackages = DocumentTemplatePackages,
            HandlersPresent = Plugins.GetPluginFeatures(typeof(DocumentHandlerProviderFeature)).Any(),
        };

        public List<UserFlag> AvailableUserFlags { get; set; }

        public IAuthorizationToken AuthorizationToken { get; set; }
        public IClaimNavigatorItem ClaimNavigator { get; set; }

        public DetailsResult UserDetails { get; set; }
        public bool HasUserPhoto { get; set; }

        public FancyTreeNode[] ClaimNavigatorFancyTreeNodes
        {
            get
            {
                var rootNode = FancyTreeNode.FromClaimNavigatorItem(ClaimNavigator, true);
                rootNode.expanded = true;

                return new FancyTreeNode[] {
                    rootNode
                };
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