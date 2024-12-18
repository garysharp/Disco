using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Interop;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Plugins.Features.DetailsProvider;
using Disco.Services.Users;
using Disco.Services.Web;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class UserController : AuthorizedDatabaseController
    {
        #region User Attachments

        [DiscoAuthorize(Claims.User.ShowAttachments)]
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Client, Duration = 172800)]
        public virtual ActionResult AttachmentDownload(int id)
        {
            var ua = Database.UserAttachments.Find(id);
            if (ua != null)
            {
                var filePath = ua.RepositoryFilename(Database);
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

        [DiscoAuthorize(Claims.User.ShowAttachments)]
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Client, Duration = 172800)]
        public virtual ActionResult AttachmentThumbnail(int id)
        {
            var ua = Database.UserAttachments.Find(id);
            if (ua != null)
            {
                if (ua.WaitForThumbnailGeneration(Database, out var thumbPath, out var mimeType))
                    return File(thumbPath, mimeType);
                else
                    return File(ClientSource.Style.Images.AttachmentTypes.MimeTypeIcons.Icon(ua.MimeType), "image/png");
            }
            return HttpNotFound("Invalid Attachment Number");
        }

        [DiscoAuthorize(Claims.User.Actions.AddAttachments), ValidateAntiForgeryToken]
        public virtual ActionResult AttachmentUpload(string id, string Domain, string comments)
        {
            id = ActiveDirectory.ParseDomainAccountId(id, Domain);

            var u = Database.Users.Find(id);
            if (u != null)
            {
                if (Request.Files.Count > 0)
                {
                    var file = Request.Files.Get(0);
                    if (file.ContentLength > 0)
                    {
                        var contentType = file.ContentType;
                        if (string.IsNullOrEmpty(contentType) || contentType.Equals("unknown/unknown", StringComparison.OrdinalIgnoreCase))
                            contentType = MimeTypes.ResolveMimeType(file.FileName);

                        if (string.IsNullOrWhiteSpace(comments))
                            comments = null;

                        var ua = new Disco.Models.Repository.UserAttachment()
                        {
                            UserId = u.UserId,
                            TechUserId = CurrentUser.UserId,
                            Filename = file.FileName,
                            MimeType = contentType,
                            Timestamp = DateTime.Now,
                            Comments = comments
                        };
                        Database.UserAttachments.Add(ua);
                        Database.SaveChanges();

                        ua.SaveAttachment(Database, file.InputStream);

                        ua.GenerateThumbnail(Database);

                        return Json(ua.Id, JsonRequestBehavior.AllowGet);
                    }
                }
                throw new Exception("No Attachment Uploaded");
            }
            throw new Exception("Invalid User Id");
        }

        [DiscoAuthorize(Claims.User.ShowAttachments)]
        public virtual ActionResult Attachment(int id)
        {
            var ua = Database.UserAttachments
                .Include(a => a.DocumentTemplate)
                .Include(a => a.TechUser)
                .Where(m => m.Id == id).FirstOrDefault();
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

        [DiscoAuthorize(Claims.User.ShowAttachments)]
        public virtual ActionResult Attachments(string id, string Domain)
        {
            id = ActiveDirectory.ParseDomainAccountId(id, Domain);

            var u = Database.Users.Include("UserAttachments.DocumentTemplate").Include("UserAttachments.TechUser").Where(m => m.UserId == id).FirstOrDefault();
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

        [DiscoAuthorizeAny(Claims.User.Actions.RemoveAnyAttachments, Claims.User.Actions.RemoveOwnAttachments)]
        public virtual ActionResult AttachmentRemove(int id)
        {
            var ua = Database.UserAttachments.Include("TechUser").Where(m => m.Id == id).FirstOrDefault();
            if (ua != null)
            {
                if (ua.TechUserId.Equals(CurrentUser.UserId, StringComparison.OrdinalIgnoreCase))
                    Authorization.RequireAny(Claims.User.Actions.RemoveAnyAttachments, Claims.User.Actions.RemoveOwnAttachments);
                else
                    Authorization.Require(Claims.User.Actions.RemoveAnyAttachments);

                ua.OnDelete(Database);
                Database.SaveChanges();
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            return Json("Invalid Attachment Number", JsonRequestBehavior.AllowGet);
        }

        #endregion

        [DiscoAuthorize(Claims.User.Actions.GenerateDocuments)]
        public virtual ActionResult GeneratePdf(string id, string Domain, string DocumentTemplateId)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrEmpty(DocumentTemplateId))
                throw new ArgumentNullException(nameof(DocumentTemplateId));

            var userId = ActiveDirectory.ParseDomainAccountId(id, Domain);

            // Obsolete: Use API\DocumentTemplate\Generate instead
            return RedirectToAction(MVC.API.DocumentTemplate.Generate(DocumentTemplateId, userId));
        }

        [DiscoAuthorize(Claims.User.Actions.GenerateDocuments)]
        public virtual ActionResult GeneratePdfPackage(string id, string Domain, string DocumentTemplatePackageId)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrEmpty(DocumentTemplatePackageId))
                throw new ArgumentNullException(nameof(DocumentTemplatePackageId));

            var userId = ActiveDirectory.ParseDomainAccountId(id, Domain);

            // Obsolete: Use API\DocumentTemplatePackage\Generate instead
            return RedirectToAction(MVC.API.DocumentTemplatePackage.Generate(DocumentTemplatePackageId, userId));
        }

        public virtual ActionResult Photo(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            userId = ActiveDirectory.ParseDomainAccountId(userId);
            var user = UserService.GetUser(userId);

            if (user == null)
                return HttpNotFound();

            var service = new DetailsProviderService(Database);

            if (!service.HasUserPhoto(user))
                return HttpNotFound();

            var photo = service.GetUserPhoto(user);

            if (photo == null)
                return HttpNotFound();

            return File(photo, "image/jpeg");
        }

    }
}
