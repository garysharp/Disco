using Disco.Models.UI.Config.Enrolment;

namespace Disco.Web.Areas.Config.Models.Enrolment
{
    public class IndexModel : ConfigEnrolmentIndexModel
    {
        public string MacSshUsername { get; set; }
        public int PendingTimeoutMinutes { get; set; }
    }
}