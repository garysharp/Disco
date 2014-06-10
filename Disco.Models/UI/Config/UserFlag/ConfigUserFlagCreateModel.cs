namespace Disco.Models.UI.Config.UserFlag
{
    public interface ConfigUserFlagCreateModel : BaseUIModel
    {
        Repository.UserFlag UserFlag { get; set; }
    }
}