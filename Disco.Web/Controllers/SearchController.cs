using Disco.Models.UI.Search;
using Disco.Services.Authorization;
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
        public virtual ActionResult Query(string term, string limit = null, bool searchDetails = false)
        {
            term = term.Trim();
            int termInt;
            if (!int.TryParse(term, out termInt))
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
                    m.Jobs = Services.Searching.Search.SearchJobsTable(Database, term, null, true, searchDetails);

                if (Authorization.Has(Claims.Device.Search))
                    m.Devices = Services.Searching.Search.SearchDevices(Database, term, null, searchDetails);

                if (Authorization.Has(Claims.User.Search))
                    m.Users = Services.Searching.Search.SearchUsers(Database, term);
            }
            else
            {
                switch (limit.ToLower())
                {
                    case "devicemodel":
                        Authorization.Require(Claims.Device.Search);
                        int deviceModelId;
                        if (int.TryParse(term, out deviceModelId))
                        {
                            var vm = Database.DeviceModels.Find(deviceModelId);
                            if (vm != null)
                            {
                                m.FriendlyTerm = string.Format("Device Model: {0}", vm.ToString());
                                m.Devices = Services.Searching.Search.SearchDeviceModel(Database, vm.Id);
                                break;
                            }
                        }
                        m.FriendlyTerm = string.Format("Device Model: {0}", term);
                        m.Success = false;
                        m.ErrorMessage = "Invalid Device Model Id";
                        break;
                    case "deviceprofile":
                        Authorization.Require(Claims.Device.Search);
                        int deviceProfileId;
                        if (int.TryParse(term, out deviceProfileId))
                        {
                            var dp = Database.DeviceProfiles.Find(deviceProfileId);
                            if (dp != null)
                            {
                                m.FriendlyTerm = string.Format("Device Profile: {0}", dp.ToString());
                                m.Devices = Services.Searching.Search.SearchDeviceProfile(Database, dp.Id);
                                break;
                            }
                        }
                        m.FriendlyTerm = string.Format("Device Profile: {0}", term);
                        m.Success = false;
                        m.ErrorMessage = "Invalid Device Profile Id";
                        break;
                    case "devicebatch":
                        Authorization.Require(Claims.Device.Search);
                        int deviceBatchId;
                        if (int.TryParse(term, out deviceBatchId))
                        {
                            var db = Database.DeviceBatches.Find(deviceBatchId);
                            if (db != null)
                            {
                                m.FriendlyTerm = string.Format("Device Batch: {0}", db.ToString());
                                m.Devices = Services.Searching.Search.SearchDeviceBatch(Database, db.Id);
                                break;
                            }
                        }
                        m.FriendlyTerm = string.Format("Device Batch: {0}", term);
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
                        m.Devices = Services.Searching.Search.SearchDevices(Database, term, null, searchDetails);
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
                        m.Jobs = Services.Searching.Search.SearchJobsTable(Database, term, null, true, searchDetails);
                        break;
                    case "users":
                        Authorization.Require(Claims.User.Search);
                        if (term.Length < 2)
                        {
                            m.Success = false;
                            m.ErrorMessage = "A search term of at least two characters is required";
                            return View(m);
                        }
                        m.Users = Services.Searching.Search.SearchUsers(Database, term);
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
                        var user = Database.Users.FirstOrDefault(u => u.Id == term);
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
                }
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<SearchQueryModel>(this.ControllerContext, m);

            return View(m);
        }

        #endregion
    }
}
