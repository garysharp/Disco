using System;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Disco.Services.Web
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class HandleErrorAttribute : FilterAttribute, IExceptionFilter
    {
        private readonly object _typeId = new object();
        private const string ViewArea = null;
        private const string ViewMaster = "_Layout";
        private const string ViewPage = "Error";

        public virtual void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            if (!filterContext.IsChildAction && (!filterContext.ExceptionHandled && filterContext.HttpContext.IsCustomErrorEnabled))
            {
                Exception ex = filterContext.Exception;
                HttpException httpException = new HttpException(null, ex);
                int httpExceptionCode = httpException.GetHttpCode();

                switch (httpExceptionCode)
                {
                    case (int)HttpStatusCode.InternalServerError: // 500
                    case (int)HttpStatusCode.Forbidden: // 403
                    case (int)HttpStatusCode.NotFound: // 403

                        filterContext.HandleException();

                        break;
                }
            }
        }

        public override object TypeId
        {
            get
            {
                return _typeId;
            }
        }
    }
}
