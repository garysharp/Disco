namespace Disco.Models.UI.Config.Enrolment
{
    public interface ConfigEnrolmentIndexModel : BaseUIModel
    {
        string MacSshUsername { get; set; }
        int PendingTimeoutMinutes { get; set; }
    }
}
