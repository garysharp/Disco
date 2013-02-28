using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;
using Disco.Data.Repository;
using System.IO;
using Disco.Models.BI.DocumentTemplates;
using Disco.Models.Interop.ActiveDirectory;

namespace Disco.BI.Extensions
{
    public static class UserExtensions
    {
        public static UserAttachment CreateAttachment(this User User, DiscoDataContext dbContext, User CreatorUser, string Filename, string MimeType, string Comments, Stream Content, DocumentTemplate DocumentTemplate = null, byte[] PdfThumbnail = null)
        {
            if (string.IsNullOrEmpty(MimeType) || MimeType.Equals("unknown/unknown", StringComparison.InvariantCultureIgnoreCase))
                MimeType = Interop.MimeTypes.ResolveMimeType(Filename);

            UserAttachment ua = new UserAttachment()
            {
                UserId = User.Id,
                TechUserId = CreatorUser.Id,
                Filename = Filename,
                MimeType = MimeType,
                Timestamp = DateTime.Now,
                Comments = Comments
            };

            if (DocumentTemplate != null)
                ua.DocumentTemplateId = DocumentTemplate.Id;

            dbContext.UserAttachments.Add(ua);
            dbContext.SaveChanges();

            ua.SaveAttachment(dbContext, Content);
            Content.Position = 0;
            if (PdfThumbnail == null)
                ua.GenerateThumbnail(dbContext, Content);
            else
                ua.SaveThumbnailAttachment(dbContext, PdfThumbnail);

            return ua;
        }

        public static List<DocumentTemplate> AvailableDocumentTemplates(this User u, DiscoDataContext dbContext, User User, DateTime TimeStamp)
        {
            var dts = dbContext.DocumentTemplates.Include("JobSubTypes")
               .Where(dt => dt.Scope == DocumentTemplate.DocumentTemplateScopes.User)
               .ToArray()
               .Where(dt => dt.FilterExpressionMatches(u, dbContext, User, TimeStamp, DocumentState.DefaultState())).ToList();

            return dts;
        }

        public static List<DeviceUserAssignment> CurrentDeviceUserAssignments(this User u)
        {
            return u.DeviceUserAssignments.Where(dua => !dua.UnassignedDate.HasValue).ToList();
        }
        public static ActiveDirectoryUserAccount ActiveDirectoryAccount(this User User, params string[] AdditionalProperties)
        {
            return Interop.ActiveDirectory.ActiveDirectory.GetUserAccount(User.Id, AdditionalProperties);
        }
    }
}
