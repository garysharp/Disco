namespace Disco.Models.Repository
{
    public interface IAttachmentTarget
    {
        string AttachmentReferenceId { get; }

        AttachmentTypes HasAttachmentType { get; }
    }
}
