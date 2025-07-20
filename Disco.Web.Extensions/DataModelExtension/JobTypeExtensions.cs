using Disco.Models.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Extensions
{
    public static class JobTypeExtensions
    {
        public static List<SelectListItem> ToSelectListItems(this IEnumerable<JobType> jobTypes, JobType SelectedItem)
        {
            string selectedId = default(string);

            if (SelectedItem != null)
                selectedId = SelectedItem.Id;

            return jobTypes.ToSelectListItems(selectedId);
        }

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<JobType> jobTypes, string SelectedId = null)
        {
            if (SelectedId == null)
                return jobTypes.Select(jt => new SelectListItem { Value = jt.Id, Text = jt.Description }).ToList();
            else
                return jobTypes.Select(jt => new SelectListItem { Value = jt.Id, Text = jt.Description, Selected = (SelectedId == jt.Id) }).ToList();
        }

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<JobType> jobTypes, IEnumerable<JobType> SelectedItems)
        {
            List<string> selectedIds = default(List<string>);

            if (SelectedItems != null)
                selectedIds = SelectedItems.Select(i => i.Id).ToList();

            return jobTypes.ToSelectListItems(selectedIds);
        }

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<JobType> jobTypes, List<string> SelectedIds = null)
        {
            if (SelectedIds == null)
                return jobTypes.Select(jt => new SelectListItem { Value = jt.Id, Text = jt.Description }).ToList();
            else
                return jobTypes.Select(jt => new SelectListItem { Value = jt.Id, Text = jt.Description, Selected = (SelectedIds.Contains(jt.Id)) }).ToList();
        }
    }
}
