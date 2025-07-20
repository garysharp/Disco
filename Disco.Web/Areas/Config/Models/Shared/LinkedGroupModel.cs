using Disco.Services.Interop.ActiveDirectory;

namespace Disco.Web.Areas.Config.Models.Shared
{
    public class LinkedGroupModel
    {
        public bool CanConfigure { get; set; }

        public string Description { get; set; }
        public string CategoryDescription { get; set; }

        public string UpdateUrl { get; set; }

        public ADManagedGroup ManagedGroup { get; set; }
        public bool IncludeFilterBeginDate { get; set; }
    }
}