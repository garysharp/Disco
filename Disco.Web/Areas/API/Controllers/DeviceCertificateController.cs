using Disco.Services.Authorization;
using Disco.Services.Web;
using System;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class DeviceCertificateController : AuthorizedDatabaseController
    {

        [DiscoAuthorize(Claims.Config.DeviceCertificate.DownloadCertificates)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Download(int id)
        {
            var wc = Database.DeviceCertificates.Find(id);
            if (wc == null)
            {
                throw new Exception("Invalid Device Certificate Id");
            }
            return File(wc.Content, "application/x-pkcs12", $"{wc.Name}.pfx");
        }

    }
}
