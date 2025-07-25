namespace Disco.Models.UI.Config.JobQueue
{
    public interface ConfigJobQueueCreateModel : BaseUIModel
    {
        string Name { get; set; }
        string Description { get; set; }
    }
}
