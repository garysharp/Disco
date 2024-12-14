using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Disco.Models.Repository;

namespace Disco.Web.Extensions
{
    public static class JobSubTypeExtensions
    {
        public static List<SelectListItem> ToSelectListItems(this IEnumerable<JobSubType> jobSubTypes, IEnumerable<JobSubType> SelectedItems, bool IncludeQueueIcons = false)
        {
            List<string> selectedIds = default(List<string>);

            if (SelectedItems != null)
                selectedIds = SelectedItems.Select(i => string.Format("{0}_{1}", i.JobTypeId, i.Id)).ToList();

            return jobSubTypes.ToSelectListItems(selectedIds);
        }

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<JobSubType> jobSubTypes, List<string> SelectedIds = null, bool IncludeQueueIcons = false)
        {
            if (SelectedIds == null)
                return jobSubTypes.Select(jst => new SelectListItem { Value = string.Format("{0}_{1}", jst.JobTypeId, jst.Id), Text = IncludeQueueIcons ? jst.DescriptionWithIcons() : jst.Description }).ToList();
            else
                return jobSubTypes.Select(jst => new SelectListItem { Value = string.Format("{0}_{1}", jst.JobTypeId, jst.Id), Text = IncludeQueueIcons ? jst.DescriptionWithIcons() : jst.Description, Selected = (SelectedIds.Contains(string.Format("{0}_{1}", jst.JobTypeId, jst.Id))) }).ToList();
        }

        public static string DescriptionWithIcons(this JobSubType jst)
        {
            if (jst.JobQueues.Count > 0)
            {
                var sb = new System.Text.StringBuilder(System.Web.HttpUtility.HtmlEncode(jst.Description));

                foreach (var jq in jst.JobQueues)
                    sb.AppendFormat("&nbsp;<i class=\"queue fa fa-{0} fa-fw d-{1}\" title=\"{2}\"></i>", jq.Icon, jq.IconColour, jq.Name);

                return sb.ToString();
            }
            else
            {
                return System.Web.HttpUtility.HtmlEncode(jst.Description);
            }
        }
    }
}
