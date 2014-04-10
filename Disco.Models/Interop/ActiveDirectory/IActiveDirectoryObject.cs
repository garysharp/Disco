
namespace Disco.Models.Interop.ActiveDirectory
{
    public interface IActiveDirectoryObject
    {
        string DistinguishedName { get; set; }
        string SecurityIdentifier { get; set; }

        string Domain { get; set; }
        string SamAccountName { get; set; }
        string NetBiosId { get; }

        string Name { get; set; }
    }
}
