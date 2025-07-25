namespace Disco.Models.UI.Config.UserFlag
{
    public interface ConfigUserFlagCreateModel : BaseUIModel
    {
        string Name { get; set; }
        string Description { get; set; }
    }
}