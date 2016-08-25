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
            // 0    | magic number = 0x0D
            // 1    | bits 0-3 = version; 4-7 = flags (1 = has document template id, 2 = device attachment,
            //      |                                  4 = job attachment, 8 = user attachment)
            // 2-3  | deployment checksum (int16)
            // 4-7  | timestamp (uint32 unix epoch)
            // 8-9  | page index (uint16)
            // 10   | creator id length
            // 11-? | creator id (UTF8)
            // ?    | target id length
            // ?-?  | target id
            // ?    | document template id length (optional based on flag)
            // ?-?  | document template id (UTF8, optional based on flag)

            var encoding = Encoding.UTF8;
            byte flags = 0;

            switch (AttachmentType)
            {
                case AttachmentTypes.Device:
                    flags = 2;
                    break;
                case AttachmentTypes.Job:
                    flags = 4;
                    break;
                case AttachmentTypes.User:
                    flags = 8;
                    break;
            }

            var deploymentChecksumBytes = BitConverter.GetBytes(DeploymentChecksum);
            var timeStampEpochBytes = BitConverter.GetBytes((uint)(TimeStamp.ToUniversalTime().Subtract(DateTime.FromFileTimeUtc(116444736000000000L)).Ticks / TimeSpan.TicksPerSecond));
            var pageIndexBytes = BitConverter.GetBytes((ushort)PageIndex);


            // magic number (1) + version/flags (1) + deployment checksum (2) + timestamp (4) +
            //  page index (2) + creator id length (1) + target id length (1)
            var requiredBytes = 12;
            var creatorIdLength = encoding.GetByteCount(CreatorId);
            var targetIdLength = encoding.GetByteCount(TargetId);
            var documentTemplateIdLength = 0;

            if (DocumentTemplateId != null)
            {
                flags |= 1;
                requiredBytes++;
                documentTemplateIdLength = encoding.GetByteCount(DocumentTemplateId);
            }

            int position = 0;
            var result = new byte[requiredBytes];

            // magic number
            result[position++] = MagicNumber;
            // version & flags
            result[position++] = (byte)(2 | (flags << 4));
            // deployment checksum
            deploymentChecksumBytes.CopyTo(result, position);
            position += 2;
            // timestamp
            timeStampEpochBytes.CopyTo(result, position);
            position += 4;
            // page index
            pageIndexBytes.CopyTo(result, position);
            position += 2;
            // creator id length
            result[position++] = (byte)creatorIdLength;
            // creator id
            position += encoding.GetBytes(CreatorId, 0, CreatorId.Length, result, position);
            // target id length
            result[position++] = (byte)targetIdLength;
            // target id
            position += encoding.GetBytes(TargetId, 0, TargetId.Length, result, position);
            if (documentTemplateIdLength > 0)
            {
                // document template id length
                result[position++] = (byte)documentTemplateIdLength;
                // document template id
                position += encoding.GetBytes(DocumentTemplateId, 0, DocumentTemplateId.Length, result, position);
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

        public static bool TryParse(DiscoDataContext Database, byte[] UniqueIdentifier, out DocumentUniqueIdentifier Identifier)
        {
            if (IsDocumentUniqueIdentifier(UniqueIdentifier))
            {
                // first 4 bit indicate version
                var version = UniqueIdentifier[2] & 0x0F;

                // Version 2
                if (version == 2)
                {
                    // Byte | Meaning
                    // 0    | magic number = 0x0D
                    // 1    | bits 0-3 = version; 4-7 = flags (1 = has document template id, 2 = device attachment,
                    //      |                                  4 = job attachment, 8 = user attachment)
                    // 2-3  | deployment checksum (int16)
                    // 4-7  | timestamp (uint32 unix epoch)
                    // 8-9  | page index (uint16)
                    // 10   | creator id length
                    // 11-? | creator id (UTF8)
                    // ?    | target id length
                    // ?-?  | target id
                    // ?    | document template id length (optional based on flag)
                    // ?-?  | document template id (UTF8, optional based on flag)
                    var encoding = Encoding.UTF8;
                    var position = 2;

                    // next 4 bits are flags
                    var flags = UniqueIdentifier[position++] >> 4;

                    var deploymentChecksum = BitConverter.ToInt16(UniqueIdentifier, position);
                    position += 2;

                    var timeStampEpoch = BitConverter.ToUInt32(UniqueIdentifier, position);
                    position += 4;

                    var pageIndex = BitConverter.ToUInt16(UniqueIdentifier, position);
                    position += 2;

                    var creatorIdLength = UniqueIdentifier[position++];
                    var creatorId = encoding.GetString(UniqueIdentifier, position, creatorIdLength);
                    position += creatorIdLength;

                    var targetIdLength = UniqueIdentifier[position++];
                    var targetId = encoding.GetString(UniqueIdentifier, position, targetIdLength);
                    position += targetIdLength;

                    string documentTemplateId = null;

                    // Has document template id flag
                    if ((flags & 1) == 1)
                    {
                        var documentTemplateIdLength = UniqueIdentifier[position++];
                        documentTemplateId = encoding.GetString(UniqueIdentifier, position, documentTemplateIdLength);
                    }

                    AttachmentTypes? attachmentType = null;
                    if ((flags & 2) == 2)
                        attachmentType = AttachmentTypes.Device;
                    else if ((flags & 4) == 4)
                        attachmentType = AttachmentTypes.Job;
                    else if ((flags & 8) == 8)
                        attachmentType = AttachmentTypes.User;

                    var timeStamp = DateTime.FromFileTimeUtc(116444736000000000L).AddTicks(TimeSpan.TicksPerSecond * timeStampEpoch).ToLocalTime();

                    Identifier = new DocumentUniqueIdentifier(Database, version, deploymentChecksum, documentTemplateId, targetId, creatorId, timeStamp, pageIndex, attachmentType);
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
