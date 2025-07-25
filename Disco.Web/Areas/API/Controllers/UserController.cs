using Disco.Models.Repository;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Interop;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Interop.DiscoServices.Upload;
using Disco.Services.Plugins.Features.DetailsProvider;
using Disco.Services.Users;
using Disco.Services.Web;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class UserController : AuthorizedDatabaseController
    {
        #region User Comments

        [DiscoAuthorize(Claims.User.ShowComments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Comments(string id, string domain)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var userId = ActiveDirectory.ParseDomainAccountId(id, domain);

            var user = Database.Users
                .Include(u => u.UserComments.Select(l => l.TechUser))
                .Where(u => u.UserId == userId).FirstOrDefault();
            if (user == null)
                return BadRequest("Invalid User Id");

            var results = user.UserComments.OrderByDescending(c => c.Timestamp).Select(c => Models.Shared.CommentModel.FromEntity(c)).ToList();
            return Json(results);
        }

        [DiscoAuthorize(Claims.User.ShowComments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Comment(int id)
        {
            var entity = Database.UserComments
                .Include(c => c.TechUser)
                .FirstOrDefault(c => c.Id == id);

            if (entity == null)
                return BadRequest("Invalid User Comment Id");

            var comment = Models.Shared.CommentModel.FromEntity(entity);
            return Json(comment);
        }

        [DiscoAuthorize(Claims.User.Actions.AddComments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult CommentAdd(string id, string domain, string comment = null)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var userId = ActiveDirectory.ParseDomainAccountId(id, domain);

            if (string.IsNullOrWhiteSpace(comment))
                return BadRequest("Comment is required");

            var user = Database.Users.Find(userId);
            if (user == null)
                return BadRequest("Invalid User Id");

            var entity = new UserComment()
            {
                UserId = user.UserId,
                TechUserId = CurrentUser.UserId,
                Timestamp = DateTime.Now,
                Comments = comment
            };
            Database.UserComments.Add(entity);
            Database.SaveChanges();

            return Json(entity.Id);
        }

        [DiscoAuthorizeAny(Claims.User.Actions.RemoveAnyComments, Claims.User.Actions.RemoveOwnComments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult CommentRemove(int id)
        {
            var entity = Database.UserComments.Find(id);
            if (entity != null)
            {
                if (entity.TechUserId.Equals(CurrentUser.UserId, StringComparison.OrdinalIgnoreCase))
                    Authorization.RequireAny(Claims.User.Actions.RemoveAnyComments, Claims.User.Actions.RemoveOwnComments);
                else
                    Authorization.Require(Claims.User.Actions.RemoveAnyComments);

                Database.UserComments.Remove(entity);
                Database.SaveChanges();
            }
            // Doesn't Exist/Already Deleted - OK
            return Ok();
        }
        #endregion

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

        [DiscoAuthorize(Claims.User.Actions.AddAttachments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult AttachmentUpload(string id, string domain, string comments)
        {
            id = ActiveDirectory.ParseDomainAccountId(id, domain);

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

                        var ua = new UserAttachment()
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
        public virtual ActionResult Attachments(string id, string domain)
        {
            id = ActiveDirectory.ParseDomainAccountId(id, domain);

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
        [HttpPost, ValidateAntiForgeryToken]
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
                return Ok();
            }
            return BadRequest("Invalid Attachment Number");
        }

        [DiscoAuthorize(Claims.User.Actions.AddAttachments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> AttachmentOnlineUploadSession(string id, string domain)
        {
            var userId = ActiveDirectory.ParseDomainAccountId(id, domain);
            if (!UserService.TryGetUser(userId, Database, false, out var user))
                throw new InvalidOperationException("Unknown User");

            try
            {
                if (!Database.DiscoConfiguration.IsActivated)
                    throw new InvalidOperationException("Activation is required to use this feature (See: Configuration > System)");

                var (uri, expiration) = await UploadOnlineService.CreateSession(CurrentUser, user);

                UploadOnlineSyncTask.ScheduleInOneHour();

                return Json(new
                {
                    Success = true,
                    Expiration = expiration.ToUnixEpoc(),
                    SessionUri = uri.ToString(),
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

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
