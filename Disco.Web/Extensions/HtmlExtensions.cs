using Disco.Services.Interop.ActiveDirectory;
using MarkdownSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace Disco.Web
{
    public static class HtmlExtensions
    {

        public static HtmlHelper GetPageHelper(this System.Web.WebPages.Html.HtmlHelper html)
        {
            return ((WebViewPage)WebPageContext.Current.Page).Html;
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
            var config = new Data.Configuration.SystemConfiguration(null);
            return new MvcHtmlString(urlHelper.Action(MVC.API.System.OrganisationLogo(Width, Height, config.OrganisationLogoHash)));
        }
        #endregion

        public static MvcHtmlString ToMultilineString(this string s)
        {
            return new MvcHtmlString(HttpUtility.HtmlEncode(s).Replace(Environment.NewLine, "<br />").Replace("\n", "<br />").Replace("\r", "<br />"));
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

        private static Lazy<Regex> htmlCommentJobRegex = new Lazy<Regex>(() => { return new Regex(@"((?<!&)#(\d+))", RegexOptions.Compiled, TimeSpan.FromSeconds(.1)); });
        private static Lazy<Regex> htmlCommentUserRegex = new Lazy<Regex>(() => { return new Regex(@"((?<!&)@([\w\d-_.]+\\)?([\w\d-_.]+[\w\d]))", RegexOptions.Compiled, TimeSpan.FromSeconds(.1)); });
        private static Lazy<Regex> htmlCommentDeviceRegex = new Lazy<Regex>(() => { return new Regex(@"((?<!&)!([\w\d-_.]+[\w\d]))", RegexOptions.Compiled, TimeSpan.FromSeconds(.1)); });
        private static IMarkdownOptions markdownOptions = new MarkdownOptions()
        {
            AutoNewLines = true,
            AutoHyperlink = true,
            LinkEmails = true
        };
        public static MvcHtmlString ToHtmlComment(this string s)
        {
            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            var html = HttpUtility.HtmlEncode(s);
            var markdownParser = new Markdown(markdownOptions);
            var markdown = markdownParser.Transform(html);

            try
            {
                // Job Matches
                markdown = htmlCommentJobRegex.Value.Replace(markdown, match =>
                {
                    if (int.TryParse(match.Groups[2].Value, out var jobId))
                        return $"<a href=\"{urlHelper.Action(MVC.Job.Show(jobId))}\" title=\"Job {jobId}\">{match.Value}</a>";
                    else
                        return match.Value;
                });

                // User Matches
                markdown = htmlCommentUserRegex.Value.Replace(markdown, match =>
                {
                    string domainId = match.Groups[2].Value;
                    string userId = match.Groups[3].Value;

                    try
                    {
                        if (string.IsNullOrWhiteSpace(userId))
                            return match.Value;
                        if (string.IsNullOrWhiteSpace(domainId))
                            userId = ActiveDirectory.ParseDomainAccountId(userId);
                        else
                            userId = string.Concat(domainId, userId);

                        if (!ActiveDirectory.IsValidDomainAccountId(userId))
                            return match.Value;

                        return $"<a href=\"{urlHelper.Action(MVC.User.Show(userId))}\" title=\"User {ActiveDirectory.FriendlyAccountId(userId)}\">{match.Value}</a>";
                    }
                    catch (Exception)
                    {
                        // Ignore incorrectly encoded User Ids
                        return match.Value;
                    }
                });

                // Device Matches
                markdown = htmlCommentDeviceRegex.Value.Replace(markdown, match =>
                {
                    string deviceSerialNumber = match.Groups[2].Value;

                    if (string.IsNullOrWhiteSpace(deviceSerialNumber))
                        return match.Value;

                    return $"<a href=\"{urlHelper.Action(MVC.Device.Show(deviceSerialNumber))}\" title=\"Device {deviceSerialNumber}\">{match.Value}</a>";
                });
            }
            catch (Exception)
            {
                // Ignore Encoding Exceptions
            }

            return new MvcHtmlString(markdown);
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