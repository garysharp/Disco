using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.BI;
using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Models.UI.Search;

namespace Disco.Web.Controllers
{
    public partial class SearchController : dbAdminController
    {
        #region Query
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
            using (dbContext = new DiscoDataContext())
            {
                if (limit == null)
                {
                    if (term.Length < 2 && termInt < 0) // < 2 Characters && Not a Number
                    {
                        m.Success = false;
                        m.ErrorMessage = "A search term of at least two characters is required";
                        return View(m);
                    }
                    m.Devices = BI.DeviceBI.Searching.Search(dbContext, term);
                    m.Jobs = BI.JobBI.Searching.Search(dbContext, term, null, true, searchDetails);
                    m.Users = BI.UserBI.Searching.Search(dbContext, term);
                }
                else
                {
                    switch (limit.ToLower())
                    {
                        case "devicemodel":
                            int deviceModelId;
                            if (int.TryParse(term, out deviceModelId))
                            {
                                var vm = dbContext.DeviceModels.Find(deviceModelId);
                                if (vm != null)
                                {
                                    m.FriendlyTerm = string.Format("Device Model: {0}", vm.ToString());
                                    m.Devices = BI.DeviceBI.Searching.SearchDeviceModel(dbContext, vm.Id);
                                    break;
                                }
                            }
                            m.FriendlyTerm = string.Format("Device Model: {0}", term);
                            m.Success = false;
                            m.ErrorMessage = "Invalid Device Model Id";
                            break;
                        case "deviceprofile":
                            int deviceProfileId;
                            if (int.TryParse(term, out deviceProfileId))
                            {
                                var dp = dbContext.DeviceProfiles.Find(deviceProfileId);
                                if (dp != null)
                                {
                                    m.FriendlyTerm = string.Format("Device Profile: {0}", dp.ToString());
                                    m.Devices = BI.DeviceBI.Searching.SearchDeviceProfile(dbContext, dp.Id);
                                    break;
                                }
                            }
                            m.FriendlyTerm = string.Format("Device Profile: {0}", term);
                            m.Success = false;
                            m.ErrorMessage = "Invalid Device Profile Id";
                            break;
                        case "devicebatch":
                            int deviceBatchId;
                            if (int.TryParse(term, out deviceBatchId))
                            {
                                var db = dbContext.DeviceBatches.Find(deviceBatchId);
                                if (db != null)
                                {
                                    m.FriendlyTerm = string.Format("Device Batch: {0}", db.ToString());
                                    m.Devices = BI.DeviceBI.Searching.SearchDeviceBatch(dbContext, db.Id);
                                    break;
                                }
                            }
                            m.FriendlyTerm = string.Format("Device Batch: {0}", term);
                            m.Success = false;
                            m.ErrorMessage = "Invalid Device Batch Id";
                            break;
                        case "devices":
                            if (term.Length < 2)
                            {
                                m.Success = false;
                                m.ErrorMessage = "A search term of at least two characters is required";
                                return View(m);
                            }
                            m.Devices = BI.DeviceBI.Searching.Search(dbContext, term);
                            if (m.Devices.Count == 1)
                            {
                                return RedirectToAction(MVC.Device.Show(m.Devices[0].SerialNumber));
                            }
                            break;
                        case "jobs":
                            if (term.Length < 2 && termInt < 0)
                            {
                                m.Success = false;
                                m.ErrorMessage = "A search term of at least two characters is required";
                                return View(m);
                            }
                            if (termInt >= 0)
                            { // Term is a Number - Check for JobId
                                if (dbContext.Jobs.Count(j => j.Id == termInt) == 1)
                                {
                                    return RedirectToAction(MVC.Job.Show(termInt));
                                }
                            }
                            m.Jobs = BI.JobBI.Searching.Search(dbContext, term, null, true, searchDetails);
                            break;
                        case "users":
                            if (term.Length < 2)
                            {
                                m.Success = false;
                                m.ErrorMessage = "A search term of at least two characters is required";
                                return View(m);
                            }
                            m.Users = BI.UserBI.Searching.Search(dbContext, term);
                            if (m.Users.Count == 1)
                            {
                                return RedirectToAction(MVC.User.Show(m.Users[0].Id));
                            }
                            break;
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
