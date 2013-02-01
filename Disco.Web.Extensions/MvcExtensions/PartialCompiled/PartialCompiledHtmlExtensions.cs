using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using System.Web.WebPages;
using System.IO;
using System.Globalization;

namespace Disco.Web.Extensions
{
    public static class PartialCompiledHtmlExtensions
    {
        #region Render Compiled Views
        private static void RenderPartialCompiledInternal(this HtmlHelper htmlHelper, Type viewType, object model, TextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            WebViewPage page = Activator.CreateInstance(viewType) as WebViewPage;
            if (page == null)
                throw new InvalidOperationException("Invalid View Type");
            page.ViewContext = htmlHelper.ViewContext;
            page.ViewData = new ViewDataDictionary(model);
            page.InitHelpers();
            HttpContextBase httpContext = htmlHelper.ViewContext.HttpContext;
            page.ExecutePageHierarchy(new WebPageContext(httpContext, null, model), writer, null);
        }
        public static void RenderPartialCompiled(this HtmlHelper htmlHelper, Type viewType)
        {
            RenderPartialCompiled(htmlHelper, viewType, null);
        }
        public static void RenderPartialCompiled(this HtmlHelper htmlHelper, Type viewType, object model)
        {
            htmlHelper.RenderPartialCompiledInternal(viewType, model, htmlHelper.ViewContext.Writer);
        }
        public static MvcHtmlString PartialCompiled(this HtmlHelper htmlHelper, Type viewType)
        {
            return PartialCompiled(htmlHelper, viewType, null);
        }
        public static MvcHtmlString PartialCompiled(this HtmlHelper htmlHelper, Type viewType, object model)
        {
            using (StringWriter writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                htmlHelper.RenderPartialCompiledInternal(viewType, model, writer);
                return MvcHtmlString.Create(writer.ToString());
            }
        }
        #endregion
    }
}
