using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.API.Models.DocumentTemplate
{
    public class ImporterUndetectedDataIdLookupModel
    {
        public string value { get; set; }
        public string label { get; set; }

        public static ImporterUndetectedDataIdLookupModel FromSearchResultItem(Disco.Models.BI.Search.DeviceSearchResultItem item)
        {
            return new ImporterUndetectedDataIdLookupModel
            {
                value = item.SerialNumber,
                label = string.Format("{0} - {1} - {2}", item.SerialNumber, item.ComputerName, item.DeviceModelDescription)
            };
        }
        public static ImporterUndetectedDataIdLookupModel FromSearchResultItem(Disco.Models.BI.Job.JobTableModel.JobTableItemModel item)
        {
            return new ImporterUndetectedDataIdLookupModel
            {
                value = item.Id.ToString(),
                label = string.Format("{0} ({1}; {2})", item.Id, item.DeviceSerialNumber, item.UserDisplayName)
            };
        }
        public static ImporterUndetectedDataIdLookupModel FromSearchResultItem(Disco.Models.BI.Search.UserSearchResultItem item)
        {
            return new ImporterUndetectedDataIdLookupModel
            {
                value = item.Id,
                label = string.Format("{0} - {1}", item.Id, item.DisplayName)
            };
        }

    }
}