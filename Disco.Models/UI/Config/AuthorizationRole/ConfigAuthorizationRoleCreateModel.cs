namespace Disco.Models.UI.Config.AuthorizationRole
{
    public interface ConfigAuthorizationRoleCreateModel : BaseUIModel
    {
        Models.Repository.AuthorizationRole AuthorizationRole { get; set; }
    }
}
