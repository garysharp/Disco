using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Disco.Models.Repository;

namespace Disco.Web.Extensions
{
    public static class JobSubTypeExtensions
    {
        public static List<SelectListItem> ToSelectListItems(this IEnumerable<JobSubType> jobSubTypes, List<JobSubType> SelectedItems)
        {
            List<string> selectedIds = default(List<string>);
            
            if (SelectedItems != null)
                selectedIds = SelectedItems.Select(i => string.Format("{0}_{1}", i.JobTypeId, i.Id)).ToList();

            return jobSubTypes.ToSelectListItems(selectedIds);
        }

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<JobSubType> jobSubTypes, List<string> SelectedIds = null)
        {
            if (SelectedIds == null)
                return jobSubTypes.Select(jst => new SelectListItem { Value = string.Format("{0}_{1}", jst.JobTypeId, jst.Id), Text = jst.Description }).ToList();
            else
                return jobSubTypes.Select(jst => new SelectListItem { Value = string.Format("{0}_{1}", jst.JobTypeId, jst.Id), Text = jst.Description, Selected = (SelectedIds.Contains(string.Format("{0}_{1}", jst.JobTypeId, jst.Id))) }).ToList();
        }
    }
}
