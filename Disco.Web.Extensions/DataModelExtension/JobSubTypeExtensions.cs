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
                selectedIds = SelectedItems.Select(i => $"{i.JobTypeId}_{i.Id}").ToList();

            return jobSubTypes.ToSelectListItems(selectedIds);
        }

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<JobSubType> jobSubTypes, List<string> SelectedIds = null, bool IncludeQueueIcons = false)
        {
            if (SelectedIds == null)
                return jobSubTypes.Select(jst => new SelectListItem { Value = $"{jst.JobTypeId}_{jst.Id}", Text = IncludeQueueIcons ? jst.DescriptionWithIcons() : jst.Description }).ToList();
            else
                return jobSubTypes.Select(jst => new SelectListItem { Value = $"{jst.JobTypeId}_{jst.Id}", Text = IncludeQueueIcons ? jst.DescriptionWithIcons() : jst.Description, Selected = (SelectedIds.Contains($"{jst.JobTypeId}_{jst.Id}")) }).ToList();
        }

        public static string DescriptionWithIcons(this JobSubType jst)
        {
            if (jst.JobQueues.Count > 0)
            {
                var sb = new System.Text.StringBuilder(System.Web.HttpUtility.HtmlEncode(jst.Description));

                foreach (var jq in jst.JobQueues)
                    sb.AppendFormat($"&nbsp;<i class=\"queue fa fa-{jq.Icon} fa-fw d-{jq.IconColour}\" title=\"{jq.Name}\"></i>");

                return sb.ToString();
            }
            else
            {
                return System.Web.HttpUtility.HtmlEncode(jst.Description);
            }
        }
    }
}
