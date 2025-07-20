using Disco.Models.UI.Search;
using Disco.Services.Authorization;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Users;
using Disco.Services.Web;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Controllers
{
    public partial class SearchController : AuthorizedDatabaseController
    {
        #region Query
        [DiscoAuthorizeAny(Claims.Job.Search, Claims.Device.Search, Claims.User.Search)]
        public virtual ActionResult Query(string term, string limit = null, bool searchDetails = false, bool includeDecommissioned = false)
        {
            term = term.Trim();
            if (!int.TryParse(term, out var termInt))
                termInt = -1;

            var m = new Models.Search.QueryModel() { Term = term };

            if (string.IsNullOrEmpty(term))
            {
                m.Success = false;
                m.ErrorMessage = "A search term is required";
                return View(m);
            }

            m.Success = true;

            // Deal with !/@/# Search Shortcuts
            if (limit == null && term.Length > 0)
            {
                switch (term[0])
                {
                    case '!':
                        term = term.Substring(1);
                        limit = "DeviceSerialNumber";
                        break;
                    case '#':
                        term = term.Substring(1);
                        limit = "JobId";
                        if (!int.TryParse(term, out termInt))
                            termInt = -1;
                        break;
                    case '@':
                        term = term.Substring(1);
                        limit = "UserId";
                        break;
                }
            }

            if (limit == null)
            {
                if (term.Length < 2 && termInt < 0) // < 2 Characters && Not a Number
                {
                    m.Success = false;
                    m.ErrorMessage = "A search term of at least two characters is required";
                    return View(m);
                }
                if (Authorization.Has(Claims.Job.Search))
                    m.Jobs = Services.Searching.Search.SearchJobsTable(Database, term, LimitCount: null, IncludeJobStatus: true, SearchDetails: searchDetails);

                if (Authorization.Has(Claims.Device.Search))
                    m.Devices = Services.Searching.Search.SearchDevices(Database, term, LimitCount: null, SearchDetails: searchDetails, includeDecommissioned: includeDecommissioned);

                if (Authorization.Has(Claims.User.Search))
                    m.Users = Services.Searching.Search.SearchUsers(Database, term, true, LimitCount: null);
            }
            else
            {
                switch (limit.ToLower())
                {
                    case "devicemodel":
                        Authorization.Require(Claims.Device.Search);
                        if (int.TryParse(term, out var deviceModelId))
                        {
                            var vm = Database.DeviceModels.Find(deviceModelId);
                            if (vm != null)
                            {
                                m.FriendlyTerm = $"Device Model: {vm.ToString()}";
                                m.Devices = Services.Searching.Search.SearchDeviceModel(Database, vm.Id);
                                break;
                            }
                        }
                        m.FriendlyTerm = $"Device Model: {term}";
                        m.Success = false;
                        m.ErrorMessage = "Invalid Device Model Id";
                        break;
                    case "deviceprofile":
                        Authorization.Require(Claims.Device.Search);
                        if (int.TryParse(term, out var deviceProfileId))
                        {
                            var dp = Database.DeviceProfiles.Find(deviceProfileId);
                            if (dp != null)
                            {
                                m.FriendlyTerm = $"Device Profile: {dp.ToString()}";
                                m.Devices = Services.Searching.Search.SearchDeviceProfile(Database, dp.Id);
                                break;
                            }
                        }
                        m.FriendlyTerm = $"Device Profile: {term}";
                        m.Success = false;
                        m.ErrorMessage = "Invalid Device Profile Id";
                        break;
                    case "devicebatch":
                        Authorization.Require(Claims.Device.Search);
                        if (int.TryParse(term, out var deviceBatchId))
                        {
                            var db = Database.DeviceBatches.Find(deviceBatchId);
                            if (db != null)
                            {
                                m.FriendlyTerm = $"Device Batch: {db.ToString()}";
                                m.Devices = Services.Searching.Search.SearchDeviceBatch(Database, db.Id);
                                break;
                            }
                        }
                        m.FriendlyTerm = $"Device Batch: {term}";
                        m.Success = false;
                        m.ErrorMessage = "Invalid Device Batch Id";
                        break;
                    case "devices":
                        Authorization.Require(Claims.Device.Search);
                        if (term.Length < 2)
                        {
                            m.Success = false;
                            m.ErrorMessage = "A search term of at least two characters is required";
                            return View(m);
                        }
                        m.Devices = Services.Searching.Search.SearchDevices(Database, term, null, searchDetails, includeDecommissioned);
                        if (m.Devices.Count == 1)
                        {
                            return RedirectToAction(MVC.Device.Show(m.Devices[0].Id));
                        }
                        break;
                    case "jobs":
                        Authorization.Require(Claims.Job.Search);
                        if (term.Length < 2 && termInt < 0)
                        {
                            m.Success = false;
                            m.ErrorMessage = "A search term of at least two characters is required";
                            return View(m);
                        }
                        if (termInt >= 0)
                        { // Term is a Number - Check for JobId
                            if (Database.Jobs.Count(j => j.Id == termInt) == 1)
                            {
                                return RedirectToAction(MVC.Job.Show(termInt));
                            }
                        }
                        m.Jobs = Services.Searching.Search.SearchJobsTable(Database, term, LimitCount: null, IncludeJobStatus: true, SearchDetails: searchDetails);
                        break;
                    case "users":
                        Authorization.Require(Claims.User.Search);
                        if (term.Length < 2)
                        {
                            m.Success = false;
                            m.ErrorMessage = "A search term of at least two characters is required";
                            return View(m);
                        }
                        m.Users = Services.Searching.Search.SearchUsers(Database, term, true, LimitCount: null);
                        if (m.Users.Count == 1)
                        {
                            return RedirectToAction(MVC.User.Show(m.Users[0].Id));
                        }
                        break;
                    case "deviceserialnumber":
                        Authorization.Require(Claims.Device.Search);
                        var device = Database.Devices.FirstOrDefault(d => d.SerialNumber == term);
                        if (device != null)
                            return RedirectToAction(MVC.Device.Show(term));
                        else
                        {
                            m.Success = false;
                            m.ErrorMessage = "Unknown Device Serial Number";
                            return View(m);
                        }
                    case "jobid":
                        Authorization.Require(Claims.Job.Search);
                        if (termInt >= 0)
                        {
                            var job = Database.Jobs.FirstOrDefault(d => d.Id == termInt);
                            if (job != null)
                                return RedirectToAction(MVC.Job.Show(termInt));
                            else
                            {
                                m.Success = false;
                                m.ErrorMessage = "Unknown Job Number";
                                return View(m);
                            }
                        }
                        else
                        {
                            m.Success = false;
                            m.ErrorMessage = "Invalid Job Number";
                            return View(m);
                        }
                    case "userid":
                        Authorization.Require(Claims.User.Search);

                        term = ActiveDirectory.ParseDomainAccountId(term);

                        var user = Database.Users.FirstOrDefault(u => u.UserId == term);
                        if (user != null)
                            return RedirectToAction(MVC.User.Show(term));
                        else
                        {
                            try
                            {
                                user = UserService.GetUser(term, Database);
                                if (user != null)
                                    return RedirectToAction(MVC.User.Show(term));
                                else
                                {
                                    m.Success = false;
                                    m.ErrorMessage = "Unknown User Id";
                                    return View(m);
                                }
                            }
                            catch (ArgumentException)
                            {
                                m.Success = false;
                                m.ErrorMessage = "Unknown User Id";
                                return View(m);
                            }
                        }
                    case "userflag":
                        Authorization.RequireAll(Claims.User.Search, Claims.User.ShowFlagAssignments);
                        if (int.TryParse(term, out var userFlagId))
                        {
                            var flag = Database.UserFlags.Find(userFlagId);
                            if (flag != null)
                            {
                                m.FriendlyTerm = $"User Flag: {flag.ToString()}";
                                m.Users = Services.Searching.Search.SearchUserFlag(Database, flag.Id);
                                break;
                            }
                        }
                        m.FriendlyTerm = $"User Flag: {term}";
                        m.Success = false;
                        m.ErrorMessage = "Invalid User Flag Id";
                        break;
                    case "deviceflag":
                        Authorization.RequireAll(Claims.Device.Search, Claims.Device.ShowFlagAssignments);
                        if (int.TryParse(term, out var deviceFlagId))
                        {
                            var flag = Database.DeviceFlags.Find(deviceFlagId);
                            if (flag != null)
                            {
                                m.FriendlyTerm = $"Device Flag: {flag.ToString()}";
                                m.Devices = Services.Searching.Search.SearchDeviceFlag(Database, flag.Id);
                                break;
                            }
                        }
                        m.FriendlyTerm = $"Device Flag: {term}";
                        m.Success = false;
                        m.ErrorMessage = "Invalid Device Flag Id";
                        break;
                }
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<SearchQueryModel>(ControllerContext, m);

            return View(m);
        }

        #endregion
    }
}
