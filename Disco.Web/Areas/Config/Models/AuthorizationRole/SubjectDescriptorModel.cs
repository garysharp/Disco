using Disco.Models.Interop.ActiveDirectory;

namespace Disco.Web.Areas.Config.Models.AuthorizationRole
{
    public class SubjectDescriptorModel
    {
        public bool IsGroup { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }

        public static SubjectDescriptorModel FromActiveDirectoryObject(IActiveDirectoryObject ADObject)
        {
            var item = new SubjectDescriptorModel()
            {
                Id = ADObject.NetBiosId,
                Name = ADObject.DisplayName
            };

            if (ADObject is ActiveDirectoryGroup)
                item.IsGroup = true;

            return item;
        }
    }
}