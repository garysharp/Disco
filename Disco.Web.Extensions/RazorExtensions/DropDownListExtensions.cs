using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Disco.Web.Extensions
{
    public static class DropDownListExtensions
    {
        public static MvcHtmlString DropDownListFor<TModel, TObject, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, Func<TModel, IEnumerable<TObject>> source, Func<TObject, string> valueGenerator, Func<TObject, string> textGenerator, string instructionMessage = null)
        {
            var selectList = source(htmlHelper.ViewData.Model)
                .Select(i => new SelectListItem() { Value = valueGenerator(i), Text = textGenerator(i) });

            if (instructionMessage != null)
            {
                selectList = new SelectListItem[]
                    { new SelectListItem { Text = instructionMessage, Value = string.Empty } }
                    .Concat(selectList);
            }

            return htmlHelper.DropDownListFor(expression, selectList);
        }
    }
}
