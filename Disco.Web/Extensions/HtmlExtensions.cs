using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Web.Routing;
//using T4MVC;
using System.Web.WebPages;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;

namespace Disco.Web
{
    public static class HtmlExtensions
    {

        public static HtmlHelper GetPageHelper(this System.Web.WebPages.Html.HtmlHelper html)
        {
            return ((System.Web.Mvc.WebViewPage)WebPageContext.Current.Page).Html;
        }

        #region T4MVC Disco Extensions
        public static MvcHtmlString ActionLinkClass(this HtmlHelper htmlHelper, string linkText, ActionResult result, string CssClasses)
        {
            return htmlHelper.ActionLink(linkText, result, new { @class = CssClasses });
        }
        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, ActionResult result, string anchorId = null, string accesskey = null)
        {
            return htmlHelper.ActionLink(linkText, result, new { id = anchorId, accesskey = accesskey });
        }
        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, ActionResult result, string anchorId)
        {
            return htmlHelper.ActionLink(linkText, result, new { id = anchorId });
        }
        public static MvcHtmlString ActionLinkButton(this HtmlHelper htmlHelper, string linkText, ActionResult result, string id, string @class)
        {
            var classValue = string.IsNullOrEmpty(@class) ? "button" : string.Concat("button ", @class);

            if (string.IsNullOrWhiteSpace(id))
            {
                return T4Extensions.ActionLink(htmlHelper, linkText, result, new { @class = classValue });
            }
            else
            {
                return T4Extensions.ActionLink(htmlHelper, linkText, result, new { @class = classValue, id = id });
            }
        }
        public static MvcHtmlString ActionLinkButton(this HtmlHelper htmlHelper, string linkText, ActionResult result, string id)
        {
            return ActionLinkButton(htmlHelper, linkText, result, id, null);
        }
        public static MvcHtmlString ActionLinkButton(this HtmlHelper htmlHelper, string linkText, ActionResult result)
        {
            return ActionLinkButton(htmlHelper, linkText, result, null);
        }
        public static MvcHtmlString ActionLinkSmallButton(this HtmlHelper htmlHelper, string linkText, ActionResult result, string id, string @class)
        {
            return ActionLinkButton(htmlHelper, linkText, result, id, string.IsNullOrEmpty(@class) ? "small" : string.Concat("small ", @class));
        }
        public static MvcHtmlString ActionLinkSmallButton(this HtmlHelper htmlHelper, string linkText, ActionResult result, string id)
        {
            return ActionLinkSmallButton(htmlHelper, linkText, result, id, null);
        }
        public static MvcHtmlString ActionLinkSmallButton(this HtmlHelper htmlHelper, string linkText, ActionResult result)
        {
            return ActionLinkSmallButton(htmlHelper, linkText, result, null, null);
        }
        public static MvcHtmlString OrganisationLogoUrl(this UrlHelper urlHelper, int Width = 256, int Height = 256)
        {
            var config = new Disco.Data.Configuration.ConfigurationContext(null);
            return new MvcHtmlString(urlHelper.Action(MVC.API.System.OrganisationLogo(Width, Height, config.OrganisationLogoHash)));
        }
        #endregion

        public static MvcHtmlString ToMultilineString(this string s)
        {
            return new MvcHtmlString(HttpUtility.HtmlEncode(s).Replace("\n", "<br />").Replace(Environment.NewLine, "<br />"));
        }

        public static List<Tuple<string, ActionResult>> ToBreadcrumb(this HtmlHelper htmlHelper, string title1, ActionResult link1, string title2 = null, ActionResult link2 = null, string title3 = null, ActionResult link3 = null, string title4 = null, ActionResult link4 = null, string title5 = null, ActionResult link5 = null)
        {
            var breadCrumbs = new List<Tuple<string, ActionResult>>();
            if (title1 != null)
            {
                breadCrumbs.Add(new Tuple<string, ActionResult>(title1, link1));
            }
            if (title2 != null)
            {
                breadCrumbs.Add(new Tuple<string, ActionResult>(title2, link2));
            }
            if (title3 != null)
            {
                breadCrumbs.Add(new Tuple<string, ActionResult>(title3, link3));
            }
            if (title4 != null)
            {
                breadCrumbs.Add(new Tuple<string, ActionResult>(title4, link4));
            }
            if (title5 != null)
            {
                breadCrumbs.Add(new Tuple<string, ActionResult>(title5, link5));
            }

            return breadCrumbs;
        }

        private static Lazy<Regex> _ToMultilineJobRefString = new Lazy<Regex>(() => { return new Regex("(?<![\\&])\\#(\\d+)"); });
        public static MvcHtmlString ToMultilineJobRefString(this string s)
        {
            var multiLineString = HttpUtility.HtmlEncode(s).Replace("\n", "<br />").Replace(Environment.NewLine, "<br />");
            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return new MvcHtmlString(_ToMultilineJobRefString.Value.Replace(multiLineString, string.Format("<a href=\"{0}?id=$1\">#$1</a>", urlHelper.Action(MVC.Job.Show(null)))));
        }

        public static IEnumerable<SelectListItem> ToSelectListItems(this IEnumerable<string> Items, string SelectedItem = null)
        {
            if (SelectedItem == null)
                return Items.Select(i => new SelectListItem() { Value = i, Text = i });
            else
                return Items.Select(i => new SelectListItem() { Value = i, Text = i, Selected = i.Equals(SelectedItem) });
        }

    }
}