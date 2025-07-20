using Disco.Models.Repository;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Authorization.Roles;
using Disco.Services.Interop.ActiveDirectory;
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
                    var oldRoleName = AuthorizationRole.Name;
                    AuthorizationRole.Name = Name;
                    UserService.UpdateAuthorizationRole(Database, AuthorizationRole);
                    AuthorizationLog.LogRoleConfiguredRenamed(AuthorizationRole, CurrentUser.UserId, oldRoleName);
                }
            }
        }

        private void UpdateClaims(AuthorizationRole AuthorizationRole, string[] ClaimKeys)
        {
            var proposedClaims = Claims.BuildClaims(ClaimKeys);

            var currentToken = RoleToken.FromAuthorizationRole(AuthorizationRole);
            var currentClaimKeys = Claims.GetClaimKeys(currentToken.Claims);
            var removedClaims = currentClaimKeys.Except(ClaimKeys).ToArray();
            var addedClaims = ClaimKeys.Except(currentClaimKeys).ToArray();

            AuthorizationRole.SetClaims(proposedClaims);
            UserService.UpdateAuthorizationRole(Database, AuthorizationRole);

            if (removedClaims.Length > 0)
                AuthorizationLog.LogRoleConfiguredClaimsRemoved(AuthorizationRole, CurrentUser.UserId, removedClaims);
            if (addedClaims.Length > 0)
                AuthorizationLog.LogRoleConfiguredClaimsAdded(AuthorizationRole, CurrentUser.UserId, addedClaims);
        }

        private void UpdateSubjects(AuthorizationRole AuthorizationRole, string[] Subjects)
        {
            string subjectIds = null;
            string[] removedSubjects = null;
            string[] addedSubjects = null;

            // Validate Subjects
            if (Subjects != null && Subjects.Length > 0)
            {
                var subjects = Subjects
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .Select(s => Tuple.Create(s, ActiveDirectory.RetrieveADObject(s, Quick: true)))
                    .Where(s => s.Item2 is ADUserAccount || s.Item2 is ADGroup)
                    .ToList();
                var invalidSubjects = subjects.Where(s => s.Item2 == null).ToList();

                if (invalidSubjects.Count > 0)
                    throw new ArgumentException(string.Format("Subjects not found: {0}", string.Join(", ", invalidSubjects)), "Subjects");

                var proposedSubjects = subjects.Select(s => s.Item2.Id).OrderBy(s => s).ToArray();
                var currentSubjects = AuthorizationRole.SubjectIds == null ? new string[0] : AuthorizationRole.SubjectIds.Split(',');
                removedSubjects = currentSubjects.Except(proposedSubjects).ToArray();
                addedSubjects = proposedSubjects.Except(currentSubjects).ToArray();

                subjectIds = string.Join(",", proposedSubjects);

                if (string.IsNullOrEmpty(subjectIds))
                    subjectIds = null;
            }

            if (AuthorizationRole.SubjectIds != subjectIds)
            {
                AuthorizationRole.SubjectIds = subjectIds;
                UserService.UpdateAuthorizationRole(Database, AuthorizationRole);

                if (removedSubjects != null && removedSubjects.Length > 0)
                    AuthorizationLog.LogRoleConfiguredSubjectsRemoved(AuthorizationRole, CurrentUser.UserId, removedSubjects);
                if (addedSubjects != null && addedSubjects.Length > 0)
                    AuthorizationLog.LogRoleConfiguredSubjectsAdded(AuthorizationRole, CurrentUser.UserId, addedSubjects);
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

        public virtual ActionResult Delete(int id, bool? redirect = false)
        {
            try
            {
                var ar = Database.AuthorizationRoles.Find(id);
                if (ar != null)
                {
                    ar.Delete(Database);

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

        [HttpPost]
        public virtual ActionResult UpdateAdministratorSubjects(string[] Subjects, bool redirect = false)
        {
            string[] proposedSubjects;
            string[] removedSubjects = null;
            string[] addedSubjects = null;

            // Validate Subjects
            if (Subjects == null || Subjects.Length == 0)
                throw new ArgumentNullException("Subjects", "At least one Id must be supplied");

            var subjects = Subjects
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Select(s => Tuple.Create(s, ActiveDirectory.RetrieveADObject(s, Quick: true)))
                .Where(s => s.Item2 is ADUserAccount || s.Item2 is ADGroup)
                .ToList();
            var invalidSubjects = subjects.Where(s => s.Item2 == null).ToList();

            if (invalidSubjects.Count > 0)
                throw new ArgumentException(string.Format("Subjects not found: {0}", string.Join(", ", invalidSubjects)), "Subjects");

            proposedSubjects = subjects.Select(s => s.Item2.Id).OrderBy(s => s).ToArray();
            var currentSubjects = UserService.AdministratorSubjectIds;
            removedSubjects = currentSubjects.Except(proposedSubjects).ToArray();
            addedSubjects = proposedSubjects.Except(currentSubjects).ToArray();

            UserService.UpdateAdministratorSubjectIds(Database, proposedSubjects);

            if (removedSubjects != null && removedSubjects.Length > 0)
                AuthorizationLog.LogAdministratorSubjectsRemoved(CurrentUser.UserId, removedSubjects);
            if (addedSubjects != null && addedSubjects.Length > 0)
                AuthorizationLog.LogAdministratorSubjectsAdded(CurrentUser.UserId, addedSubjects);

            if (redirect)
                return RedirectToAction(MVC.Config.AuthorizationRole.Index());
            else
                return Json("OK");
        }

        #endregion
    }
}
