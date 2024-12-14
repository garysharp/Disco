namespace Disco.Models.UI.Config.JobQueue
{
    public interface ConfigJobQueueCreateModel : BaseUIModel
    {
        Repository.JobQueue JobQueue { get; set; }
    }
}
