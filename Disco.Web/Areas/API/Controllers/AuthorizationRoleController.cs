using Disco.BI.Extensions;
using Disco.BI.Interop.ActiveDirectory;
using Disco.Models.Interop.ActiveDirectory;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Users;
using Disco.Services.Web;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    [DiscoAuthorize(Claims.DiscoAdminAccount)]
    public partial class AuthorizationRoleController : AuthorizedDatabaseController
    {

        #region Properties

        const string pName = "name";

        public virtual ActionResult Update(int id, string key, string value = null, bool redirect = false)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");
                var authorizationRole = Database.AuthorizationRoles.Find(id);
                if (authorizationRole != null)
                {
                    switch (key.ToLower())
                    {
                        case pName:
                            UpdateName(authorizationRole, value);
                            break;
                        default:
                            throw new Exception("Invalid Update Key");
                    }
                }
                else
                {
                    return Json("Invalid Authorization Role Id", JsonRequestBehavior.AllowGet);
                }
                if (redirect)
                    return RedirectToAction(MVC.Config.AuthorizationRole.Index(authorizationRole.Id));
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        private void UpdateName(AuthorizationRole AuthorizationRole, string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentNullException("Name", "Authorization Role Name is required");
            else
            {
                if (AuthorizationRole.Name != Name)
                {
                    // Check for Duplicates
                    var d = Database.AuthorizationRoles.Where(db => db.Id != AuthorizationRole.Id && db.Name == Name).Count();
                    if (d > 0)
                    {
                        throw new Exception("An Authorization Role with that name already exists");
                    }

                    AuthorizationRole.Name = Name;
                    UserService.UpdateAuthorizationRole(Database, AuthorizationRole);
                }
            }
        }

        private void UpdateClaims(AuthorizationRole AuthorizationRole, string[] ClaimKeys)
        {
            var claims = Claims.BuildClaims(ClaimKeys);
            AuthorizationRole.SetClaims(claims);

            UserService.UpdateAuthorizationRole(Database, AuthorizationRole);
        }

        private void UpdateSubjects(AuthorizationRole AuthorizationRole, string[] Subjects)
        {
            string subjectIds = null;

            // Validate Subjects
            if (Subjects != null && Subjects.Length > 0)
            {
                var subjects = Subjects.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).Select(s => new Tuple<string, IActiveDirectoryObject>(s, ActiveDirectory.GetObject(s))).ToList();
                var invalidSubjects = subjects.Where(s => s.Item2 == null).ToList();

                if (invalidSubjects.Count > 0)
                    throw new ArgumentException(string.Format("Subjects not found: {0}", string.Join(", ", invalidSubjects)), "Subjects");

                subjectIds = string.Join(",", subjects.Select(s => s.Item2.SamAccountName).OrderBy(s => s));

                if (string.IsNullOrEmpty(subjectIds))
                    subjectIds = null;
            }

            if (AuthorizationRole.SubjectIds != subjectIds)
            {
                AuthorizationRole.SubjectIds = subjectIds;
                UserService.UpdateAuthorizationRole(Database, AuthorizationRole);
            }
        }

        public virtual ActionResult UpdateName(int id, string RoleName = null, bool redirect = false)
        {
            return Update(id, pName, RoleName, redirect);
        }

        public virtual ActionResult UpdateClaims(int id, string[] ClaimKeys = null, bool redirect = false)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");

                var authorizationRole = Database.AuthorizationRoles.Find(id);
                if (authorizationRole != null)
                {
                    UpdateClaims(authorizationRole, ClaimKeys);
                }
                else
                {
                    return Json("Invalid Authorization Role Id", JsonRequestBehavior.AllowGet);
                }
                if (redirect)
                    return RedirectToAction(MVC.Config.AuthorizationRole.Index(authorizationRole.Id));
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        public virtual ActionResult UpdateSubjects(int id, string[] Subjects = null, bool redirect = false)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");

                var authorizationRole = Database.AuthorizationRoles.Find(id);
                if (authorizationRole != null)
                {
                    UpdateSubjects(authorizationRole, Subjects);
                }
                else
                {
                    return Json("Invalid Authorization Role Id", JsonRequestBehavior.AllowGet);
                }
                if (redirect)
                    return RedirectToAction(MVC.Config.AuthorizationRole.Index(authorizationRole.Id));
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Actions

        public virtual ActionResult Delete(int id, Nullable<bool> redirect = false)
        {
            try
            {
                var ar = Database.AuthorizationRoles.Find(id);
                if (ar != null)
                {
                    ar.Delete(Database);
                    Database.SaveChanges();

                    if (redirect.HasValue && redirect.Value)
                        return RedirectToAction(MVC.Config.AuthorizationRole.Index(null));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                throw new Exception("Invalid Authorization Role Id");
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        public virtual ActionResult SearchSubjects(string term)
        {
            var groupResults = BI.Interop.ActiveDirectory.ActiveDirectory.SearchGroups(term).Cast<IActiveDirectoryObject>();
            var userResults = BI.Interop.ActiveDirectory.ActiveDirectory.SearchUsers(term).Cast<IActiveDirectoryObject>();

            var results = groupResults.Concat(userResults).OrderBy(r => r.SamAccountName)
                .Select(r => Models.AuthorizationRole.SubjectItem.FromActiveDirectoryObject(r)).ToList();

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult Subject(string Id)
        {
            var subject = ActiveDirectory.GetObject(Id);
            
            if (subject == null || !(subject is ActiveDirectoryUserAccount || subject is ActiveDirectoryGroup))
                return Json(null, JsonRequestBehavior.AllowGet);
            else
                return Json(Models.AuthorizationRole.SubjectItem.FromActiveDirectoryObject(subject), JsonRequestBehavior.AllowGet);
        }
    }
}
