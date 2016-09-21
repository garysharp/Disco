using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Interop.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace Disco.Services.Documents
{
    public class DocumentUniqueIdentifier
    {
        private const int CurrentVersion = 2;
        private const byte MagicNumber = 0xC4;
        private DiscoDataContext database;

        public int Version { get; private set; }
        public short DeploymentChecksum { get; private set; }
        public string DocumentTemplateId { get; private set; }
        public string TargetId { get; private set; }
        public string CreatorId { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public int PageIndex { get; private set; }

        private DocumentTemplate documentTemplate;
        private AttachmentTypes? attachmentType;
        private IAttachmentTarget target;
        private User creator;

        public DocumentTemplate DocumentTemplate
        {
            get
            {
                if (documentTemplate == null && !string.IsNullOrWhiteSpace(DocumentTemplateId) && !DocumentTemplateId.StartsWith("--", StringComparison.Ordinal))
                {
                    documentTemplate = database.DocumentTemplates.Find(DocumentTemplateId);
                    if (documentTemplate != null)
                    {
                        attachmentType = documentTemplate.AttachmentType;
                        DocumentTemplateId = documentTemplate.Id;
                    }
                }

                return documentTemplate;
            }
        }

        public string DocumentGroupingId
        {
            get
            {
                // Unique identifier to distinguish this document from others (but keep pages together)
                return $"{TimeStamp.Ticks}|{TargetId}|{DocumentTemplateId}|{CreatorId}|{Version}|{DeploymentChecksum}";
            }
        }

        public AttachmentTypes? AttachmentType
        {
            get
            {
                if (!attachmentType.HasValue)
                {
                    if (DocumentTemplateId.StartsWith("--", StringComparison.Ordinal))
                    {
                        if (DocumentTemplateId.Equals("--DEVICE", StringComparison.Ordinal))
                        {
                            attachmentType = AttachmentTypes.Device;
                        }
                        else if (DocumentTemplateId.Equals("--JOB", StringComparison.Ordinal))
                        {
                            attachmentType = AttachmentTypes.Job;
                        }
                        else if (DocumentTemplateId.Equals("--USER", StringComparison.Ordinal))
                        {
                            attachmentType = AttachmentTypes.User;
                        }
                    }
                    else
                    {
                        var dt = DocumentTemplate;
                        if (dt != null)
                        {
                            attachmentType = dt.AttachmentType;
                        }
                    }
                }

                return attachmentType;
            }
        }

        public IAttachmentTarget Target
        {
            get
            {
                if (target == null)
                {
                    switch (AttachmentType)
                    {
                        case AttachmentTypes.Device:
                            target = database.Devices.Find(TargetId);
                            break;
                        case AttachmentTypes.Job:
                            target = database.Jobs.Find(int.Parse(TargetId));
                            break;
                        case AttachmentTypes.User:
                            target = database.Users.Find(ActiveDirectory.ParseDomainAccountId(TargetId));
                            break;
                        default:
                            throw new ArgumentException("Unexpected Attachment Type", nameof(AttachmentType));
                    }
                    if (target != null)
                    {
                        TargetId = target.AttachmentReferenceId;
                    }
                }

                return target;
            }
        }

        public User Creator
        {
            get
            {
                if (creator == null)
                {
                    creator = database.Users.Find(ActiveDirectory.ParseDomainAccountId(CreatorId));
                    if (creator != null)
                    {
                        CreatorId = creator.UserId;
                    }
                }
                return creator;
            }
        }

        private DocumentUniqueIdentifier(DiscoDataContext Database, int Version, short DeploymentChecksum, string DocumentTemplateId, string TargetId, string CreatorId, DateTime TimeStamp, int PageIndex, AttachmentTypes? AttachmentType)
        {
            this.database = Database;
            this.Version = Version;
            this.DeploymentChecksum = DeploymentChecksum;
            this.DocumentTemplateId = DocumentTemplateId;
            this.attachmentType = AttachmentType;
            this.TargetId = TargetId;
            this.CreatorId = ActiveDirectory.ParseDomainAccountId(CreatorId);
            this.TimeStamp = TimeStamp;
            this.PageIndex = PageIndex;
        }

        public string ToQRCodeString()
        {
            return $"Disco|1|{DocumentTemplate.Id}|{TargetId}|{CreatorId}|{TimeStamp:s}|{PageIndex}";
        }

        public string ToJson()
        {
            var builder = new StringBuilder();
            using (var stringWriter = new StringWriter(builder))
            {
                using (var writer = new JsonTextWriter(stringWriter))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartObject();
                    writer.WritePropertyName("version");
                    writer.WriteValue(CurrentVersion);
                    writer.WritePropertyName("attachmentType");
                    writer.WriteValue(AttachmentType.Value.ToString());
                    writer.WritePropertyName("documentTemplateId");
                    writer.WriteValue(DocumentTemplateId);
                    writer.WritePropertyName("targetId");
                    writer.WriteValue(TargetId);
                    writer.WritePropertyName("creatorId");
                    writer.WriteValue(CreatorId);
                    writer.WritePropertyName("timestamp");
                    writer.WriteValue(TimeStamp);
                    writer.WritePropertyName("pageIndex");
                    writer.WriteValue(PageIndex);
                    writer.WriteEndObject();
                }
            }
            return builder.ToString();
        }

        public byte[] ToQRCodeBytes()
        {
            // Byte | Meaning
            // 0    | magic number = 0xC4
            // 1    | bits 0-3 = version;
            //      |      4   = flag: has document template id
            //      |      5-6 = (01 = device attachment, 10 = job attachment, 11 = user attachment)
            //      |      7   = not used
            // 2-3  | deployment checksum (int16)
            // 4-7  | timestamp (uint32 unix epoch)
            // 8-9  | page index (uint16)
            // 10   | creator id encoded
            // ?    | target id encoded
            // ?    | document template id encoded (optional based on flag)
            const int version = 2;

            // encode variable-length parameters first

            // encode creator id (strip default domain)
            var creatorIdBytes = this.BinaryEncode(ActiveDirectory.FriendlyAccountId(CreatorId));

            // encode target id
            byte[] targetIdBytes;
            if (AttachmentType.HasValue && AttachmentType.Value == AttachmentTypes.User)
            {
                // strip default domain from user targetted attachments
                targetIdBytes = this.BinaryEncode(ActiveDirectory.FriendlyAccountId(TargetId));
            }
            else
            {
                targetIdBytes = this.BinaryEncode(TargetId);
            }

            byte[] documentTemplateIdBytes = null;
            if (DocumentTemplateId != null)
            {
                documentTemplateIdBytes = this.BinaryEncode(DocumentTemplateId);
            }

            var result = new byte[10 + creatorIdBytes.Length + targetIdBytes.Length + (documentTemplateIdBytes?.Length ?? 0)];

            // write magic number
            result[0] = MagicNumber;

            // write version
            result[1] = (version << 4);

            // write 'has document template id' flag
            if (documentTemplateIdBytes != null)
            {
                result[1] |= 0x8; // 0000 1000
            }

            // write attachment type
            switch (AttachmentType)
            {
                case AttachmentTypes.Device:
                    result[1] |= 0x2; // 0000 0010 - 01
                    break;
                case AttachmentTypes.Job:
                    result[1] |= 0x4; // 0000 0100 - 10
                    break;
                case AttachmentTypes.User:
                    result[1] |= 0x6; // 0000 0110 - 11
                    break;
            }

            // write deployment checksum
            result[2] = (byte)(DeploymentChecksum >> 8);
            result[3] = (byte)DeploymentChecksum;

            // write timestamp
            var timestamp = (uint)(TimeStamp.ToUniversalTime().Subtract(DateTime.FromFileTimeUtc(116444736000000000L)).Ticks / TimeSpan.TicksPerSecond);
            result[4] = (byte)(timestamp >> 24);
            result[5] = (byte)(timestamp >> 16);
            result[6] = (byte)(timestamp >> 8);
            result[7] = (byte)timestamp;

            // write page index
            result[8] = (byte)(PageIndex >> 8);
            result[9] = (byte)PageIndex;

            // write creator id
            creatorIdBytes.CopyTo(result, 10);

            // write target id
            targetIdBytes.CopyTo(result, 10 + creatorIdBytes.Length);

            // write document template id
            if (documentTemplateIdBytes != null)
            {
                documentTemplateIdBytes.CopyTo(result, 10 + creatorIdBytes.Length + targetIdBytes.Length);
            }

            return result;
        }

        public static DocumentUniqueIdentifier Create(DiscoDataContext Database, DocumentTemplate DocumentTemplate, IAttachmentTarget Target, User Creator, DateTime TimeStamp, int PageIndex)
        {
            var deploymentChecksum = Database.DiscoConfiguration.DeploymentChecksum;
            var documentTemplateId = DocumentTemplate.Id;
            var targetId = Target.AttachmentReferenceId;
            var creatorId = Creator.UserId;
            var attachmentType = DocumentTemplate.AttachmentType;

            var identifier = new DocumentUniqueIdentifier(Database, CurrentVersion, deploymentChecksum, documentTemplateId, targetId, creatorId, TimeStamp, PageIndex, attachmentType);

            identifier.documentTemplate = DocumentTemplate;
            identifier.target = Target;
            identifier.creator = Creator;

            return identifier;
        }

        public static DocumentUniqueIdentifier Create(DiscoDataContext Database, string DocumentTemplateId, string TargetId, string CreatorId, DateTime TimeStamp, int PageIndex)
        {
            var deploymentChecksum = Database.DiscoConfiguration.DeploymentChecksum;

            return new DocumentUniqueIdentifier(Database, CurrentVersion, deploymentChecksum, DocumentTemplateId, TargetId, CreatorId, TimeStamp, PageIndex, null);
        }

        public static DocumentUniqueIdentifier Parse(DiscoDataContext Database, byte[] UniqueIdentifier)
        {
            DocumentUniqueIdentifier identifier;
            if (TryParse(Database, UniqueIdentifier, out identifier))
            {
                return identifier;
            }
            else
            {
                throw new FormatException("Invalid Document Unique Identifier");
            }
        }

        public static DocumentUniqueIdentifier Parse(DiscoDataContext Database, string UniqueIdentifier)
        {
            DocumentUniqueIdentifier identifier;
            if (TryParse(Database, UniqueIdentifier, out identifier))
            {
                return identifier;
            }
            else
            {
                throw new FormatException("Invalid Document Unique Identifier");
            }
        }

        public static bool TryParse(DiscoDataContext Database, string UniqueIdentifier, out DocumentUniqueIdentifier Identifier)
        {
            if (IsDocumentUniqueIdentifier(UniqueIdentifier))
            {
                var components = UniqueIdentifier.Split('|');

                // Version 0, Version 1 Handling
                if ((components.Length == 7 || components.Length == 6) &&
                    (components[1].Equals("1", StringComparison.Ordinal) || components[1].Equals("AT", StringComparison.OrdinalIgnoreCase)))
                {
                    var documentTemplateId = components[2];
                    var targetId = components[3];
                    var creatorId = components[4];
                    var timeStamp = DateTime.Parse(components[5]);
                    var page = 0;
                    if (components.Length == 7)
                    {
                        page = int.Parse(components[6]);
                    }

                    Identifier = new DocumentUniqueIdentifier(Database, 1, -1, documentTemplateId, targetId, creatorId, timeStamp, page, null);
                    return true;
                }
            }

            Identifier = null;
            return false;
        }

        public static bool TryParse(DiscoDataContext Database, byte[] Data, out DocumentUniqueIdentifier Identifier)
        {
            if (IsDocumentUniqueIdentifier(Data))
            {
                // first 4 bit indicate version
                var version = Data[1] >> 4;

                // Version 2
                if (version == 2)
                {
                    // Byte | Meaning
                    // 0    | magic number = 0xC4
                    // 1    | bits 0-3 = version;
                    //      |      4   = flag: has document template id
                    //      |      5-6 = (01 = device attachment, 10 = job attachment, 11 = user attachment)
                    //      |      7   = not used
                    // 2-3  | deployment checksum (int16)
                    // 4-7  | timestamp (uint32 unix epoch)
                    // 8-9  | page index (uint16)
                    // 10   | creator id encoded
                    // ?    | target id encoded
                    // ?    | document template id encoded (optional based on flag)

                    // read flags
                    var flags = Data[1] & 0x0F;

                    // read deployment checksum
                    short deploymentChecksum = (short)((Data[2] << 8) | Data[3]);

                    // read timestamp
                    var timeStampEpoch = ((long)Data[4] << 24) |
                        ((long)Data[5] << 16) |
                        ((long)Data[6] << 8) |
                        Data[7];

                    // write page index
                    var pageIndex = (Data[8] << 8) | Data[9];

                    var position = 10;

                    // write creator id
                    var creatorId = DocumentUniqueIdentifierExtensions.BinaryDecode(Data, position, out position);

                    // write target id
                    var targetId = DocumentUniqueIdentifierExtensions.BinaryDecode(Data, position, out position);

                    // write document template id
                    string documentTemplateId = null;

                    // Has document template id flag
                    if ((flags & 0x8) == 0x8)
                    {
                        documentTemplateId = DocumentUniqueIdentifierExtensions.BinaryDecode(Data, position, out position);
                    }

                    AttachmentTypes? attachmentType = null;
                    switch (flags & 0x6)
                    {
                        case 0x2:
                            attachmentType = AttachmentTypes.Device;
                            break;
                        case 0x4:
                            attachmentType = AttachmentTypes.Job;
                            break;
                        case 0x6:
                            attachmentType = AttachmentTypes.User;
                            break;
                    }

                    var timeStamp = DateTime.FromFileTimeUtc(116444736000000000L).AddTicks(TimeSpan.TicksPerSecond * timeStampEpoch).ToLocalTime();

                    Identifier = new DocumentUniqueIdentifier(Database, version, deploymentChecksum, documentTemplateId, targetId, creatorId, timeStamp, pageIndex, attachmentType);
                    return true;
                }
            }

            Identifier = null;
            return false;
        }

        public static bool IsDocumentUniqueIdentifier(string Identifier)
        {
            return Identifier != null && Identifier.StartsWith("Disco|", System.StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsDocumentUniqueIdentifier(byte[] Identifier)
        {
            // Identifier[0] = 0xC4; Magic number to identify Disco ICT QR Codes
            return Identifier != null && Identifier.Length > 2 && Identifier[0] == MagicNumber;
        }
    }
}
