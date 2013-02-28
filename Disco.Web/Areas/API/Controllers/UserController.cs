using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.BI;
using Disco.BI.Extensions;
using System.IO;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class UserController : dbAdminController
    {
        public virtual ActionResult UpstreamUsers(string term)
        {
            return Json(BI.UserBI.Searching.SearchUpstream(term), JsonRequestBehavior.AllowGet);
        }

        #region User Attachements
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Client, Duration = 172800)]
        public virtual ActionResult AttachmentDownload(int id)
        {
            var ua = dbContext.UserAttachments.Find(id);
            if (ua != null)
            {
                var filePath = ua.RepositoryFilename(dbContext);
                if (System.IO.File.Exists(filePath))
                {
                    return File(filePath, ua.MimeType, ua.Filename);
                }
                else
                {
                    return HttpNotFound("Attachment reference exists, but file not found");
                }
            }
            return HttpNotFound("Invalid Attachment Number");
        }
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Client, Duration = 172800)]
        public virtual ActionResult AttachmentThumbnail(int id)
        {
            var ua = dbContext.UserAttachments.Find(id);
            if (ua != null)
            {
                var thumbPath = ua.RepositoryThumbnailFilename(dbContext);
                if (System.IO.File.Exists(thumbPath))
                {
                    if (thumbPath.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                        return File(thumbPath, "image/png");
                    else
                        return File(thumbPath, "image/jpg");
                }
                else
                    return File(ClientSource.Style.Images.AttachmentTypes.MimeTypeIcons.Icon(ua.MimeType), "image/png");
            }
            return HttpNotFound("Invalid Attachment Number");
        }
        public virtual ActionResult AttachmentUpload(string id, string Comments)
        {
            var u = dbContext.Users.Find(id);
            if (u != null)
            {
                if (Request.Files.Count > 0)
                {
                    var file = Request.Files.Get(0);
                    if (file.ContentLength > 0)
                    {
                        var contentType = file.ContentType;
                        if (string.IsNullOrEmpty(contentType) || contentType.Equals("unknown/unknown", StringComparison.InvariantCultureIgnoreCase))
                            contentType = BI.Interop.MimeTypes.ResolveMimeType(file.FileName);

                        var ua = new Disco.Models.Repository.UserAttachment()
                        {
                            UserId = u.Id,
                            TechUserId = DiscoApplication.CurrentUser.Id,
                            Filename = file.FileName,
                            MimeType = contentType,
                            Timestamp = DateTime.Now,
                            Comments = Comments
                        };
                        dbContext.UserAttachments.Add(ua);
                        dbContext.SaveChanges();

                        ua.SaveAttachment(dbContext, file.InputStream);

                        ua.GenerateThumbnail(dbContext);

                        return Json(ua.Id, JsonRequestBehavior.AllowGet);
                    }
                }
                throw new Exception("No Attachment Uploaded");
            }
            throw new Exception("Invalid User Id");
        }
        public virtual ActionResult Attachment(int id)
        {
            var ua = dbContext.UserAttachments.Include("TechUser").Where(m => m.Id == id).FirstOrDefault();
            if (ua != null)
            {

                var m = new Models.Attachment.AttachmentModel()
                {
                    Attachment = Models.Attachment._AttachmentModel.FromAttachment(ua),
                    Result = "OK"
                };

                return Json(m, JsonRequestBehavior.AllowGet);
            }
            return Json(new Models.Attachment.AttachmentModel() { Result = "Invalid Attachment Number" }, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult Attachments(string id)
        {
            var u = dbContext.Users.Include("UserAttachments.TechUser").Where(m => m.Id == id).FirstOrDefault();
            if (u != null)
            {
                var m = new Models.Attachment.AttachmentsModel()
                {
                    Attachments = u.UserAttachments.Select(ua => Models.Attachment._AttachmentModel.FromAttachment(ua)).ToList(),
                    Result = "OK"
                };

                return Json(m, JsonRequestBehavior.AllowGet);
            }
            return Json(new Models.Attachment.AttachmentsModel() { Result = "Invalid User Id" }, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult AttachmentRemove(int id)
        {
            var ua = dbContext.UserAttachments.Include("TechUser").Where(m => m.Id == id).FirstOrDefault();
            if (ua != null)
            {
                // 2012-02-17 G# Remove - 'Delete Own Comments' policy
                //if (ua.TechUserId == DiscoApplication.CurrentUser.Id)
                //{
                ua.OnDelete(dbContext);
                dbContext.SaveChanges();
                return Json("OK", JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    return Json("You can only delete your own attachments.", JsonRequestBehavior.AllowGet);
                //}
            }
            return Json("Invalid Attachment Number", JsonRequestBehavior.AllowGet);
        }

        #endregion

        public virtual ActionResult GeneratePdf(string id, string DocumentTemplateId)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(DocumentTemplateId))
                throw new ArgumentNullException("AttachmentTypeId");
            var user = dbContext.Users.Find(id);
            if (user != null)
            {
                var documentTemplate = dbContext.DocumentTemplates.Find(DocumentTemplateId);
                if (documentTemplate != null)
                {
                    var timeStamp = DateTime.Now;
                    Stream pdf;
                    using (var generationState = Disco.Models.BI.DocumentTemplates.DocumentState.DefaultState())
                    {
                        pdf = documentTemplate.GeneratePdf(dbContext, user, DiscoApplication.CurrentUser, timeStamp, generationState);
                    }
                    dbContext.SaveChanges();
                    return File(pdf, "application/pdf", string.Format("{0}_{1}_{2:yyyyMMdd-HHmmss}.pdf", documentTemplate.Id, user.Id, timeStamp));
                }
                else
                {
                    throw new ArgumentException("Invalid Document Template Id", "id");
                }
            }
            else
            {
                throw new ArgumentException("Invalid User Id", "id");
            }
        }

    }
}
