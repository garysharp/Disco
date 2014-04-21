
using System.Security.Principal;
namespace Disco.Services.Interop.ActiveDirectory
{
    public interface IADObject
    {
        ADDomain Domain { get; }

        string DistinguishedName { get; }
        SecurityIdentifier SecurityIdentifier { get; }
        
        string Id { get; }
        string SamAccountName { get; }

        string Name { get; }
        string DisplayName { get; }
    }
}
