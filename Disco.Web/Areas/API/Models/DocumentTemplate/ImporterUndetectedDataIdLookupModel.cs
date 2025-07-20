using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.Services.Searching;

namespace Disco.Web.Areas.API.Models.DocumentTemplate
{
    public class ImporterUndetectedDataIdLookupModel
    {
        public string value { get; set; }
        public string label { get; set; }

        public static ImporterUndetectedDataIdLookupModel FromSearchResultItem(DeviceSearchResultItem item)
        {
            return new ImporterUndetectedDataIdLookupModel
            {
                value = item.Id,
                label = $"{item.Id} - {item.ComputerName} - {item.DeviceModelDescription}"
            };
        }
        public static ImporterUndetectedDataIdLookupModel FromSearchResultItem(JobTableItemModel item)
        {
            return new ImporterUndetectedDataIdLookupModel
            {
                value = item.JobId.ToString(),
                label = $"{item.JobId} ({item.DeviceSerialNumber}; {item.UserDisplayName})"
            };
        }
        public static ImporterUndetectedDataIdLookupModel FromSearchResultItem(UserSearchResultItem item)
        {
            return new ImporterUndetectedDataIdLookupModel
            {
                value = item.Id,
                label = $"{item.Id} - {item.DisplayName}"
            };
        }

    }
}