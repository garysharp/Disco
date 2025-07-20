namespace Disco.Models.UI.Config.AuthorizationRole
{
    public interface ConfigAuthorizationRoleCreateModel : BaseUIModel
    {
        Repository.AuthorizationRole AuthorizationRole { get; set; }
    }
}
