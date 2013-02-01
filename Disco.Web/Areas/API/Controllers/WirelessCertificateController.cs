using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class WirelessCertificateController : dbAdminController
    {

        public virtual ActionResult Download(int id)
        {
            var wc = dbContext.DeviceCertificates.Find(id);
            if (wc == null)
            {
                throw new Exception("Invalid Wireless Certificate Number");
            }
            return File(wc.Content, "application/x-pkcs12", string.Format("{0}.pfx", wc.Name));
        }

        //public virtual ActionResult DownloadLog()
        //{
        //    //var path = BI.Wireless.BaseWirelessProvider.LoggingPath;
        //    //if (path != null && System.IO.File.Exists(path))
        //    //{
        //    //    System.IO.MemoryStream ms = new System.IO.MemoryStream();
        //    //    using (var s = new System.IO.FileStream(path,  System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
        //    //    {
        //    //        s.CopyTo(ms);
        //    //    }
        //    //    ms.Position = 0;
        //    //    return File(ms, "text/plain", "WirelessCertificateLog.txt");
        //    //}
        //    //return HttpNotFound();
        //}

        public virtual ActionResult FillBuffer(int Value, Nullable<bool> redirect = null)
        {
            throw new NotImplementedException();
            //try
            //{
            //    if (Value >= 0)
            //    {
            //        if (!WirelessCertificatesLog.IsCertificateRetrievalProcessing)
            //        {
            //            BaseWirelessProvider.GetProvider(dbContext).FillCertificateBuffer(Value);
            //            if (redirect.HasValue && redirect.Value)
            //                return RedirectToAction(MVC.Config.WirelessCertificate.Index());
            //            else
            //                return Json("OK", JsonRequestBehavior.AllowGet);
            //        }
            //        else
            //        {
            //            throw new Exception("Buffer is already processing");
            //        }
            //    }
            //    else
            //    {
            //        throw new Exception("The value must be >= 0");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    if (redirect.HasValue && redirect.Value)
            //        throw;
            //    else
            //        return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            //}
        }

        //#region Auto Buffer

        //public virtual ActionResult AutoBufferMax(int Value, Nullable<bool> redirect = null)
        //{
        //    try
        //    {
        //        if (Value >= 0)
        //        {
        //            dbContext.DiscoConfiguration.Wireless.CertificateAutoBufferMax = Value;
        //            dbContext.SaveChanges();
        //            if (redirect.HasValue && redirect.Value)
        //                return RedirectToAction(MVC.Config.WirelessCertificate.Index());
        //            else
        //                return Json("OK", JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            throw new Exception("The value must be >= 0");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (redirect.HasValue && redirect.Value)
        //            throw;
        //        else
        //            return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
        //    }
        //}
        //public virtual ActionResult AutoBufferLow(int Value, Nullable<bool> redirect = null)
        //{
        //    try
        //    {
        //        if (Value >= 0)
        //        {
        //            dbContext.DiscoConfiguration.Wireless.CertificateAutoBufferLow = Value;
        //            dbContext.SaveChanges();
        //            if (redirect.HasValue && redirect.Value)
        //                return RedirectToAction(MVC.Config.WirelessCertificate.Index());
        //            else
        //                return Json("OK", JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            throw new Exception("The value must be >= 0");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (redirect.HasValue && redirect.Value)
        //            throw;
        //        else
        //            return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
        //    }
        //}

        //#endregion

        //#region eduSTAR Credentials

        //public virtual ActionResult eduSTAR_SchoolId(string SchoolId)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrWhiteSpace(SchoolId))
        //        {
        //            dbContext.DiscoConfiguration.Wireless.eduSTAR_ServiceAccountSchoolId = SchoolId;
        //            dbContext.SaveChanges();
        //            return Json("OK", JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            throw new Exception("The SchoolId cannot be null or empty");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
        //    }
        //}
        //public virtual ActionResult eduSTAR_Username(string Username)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrWhiteSpace(Username))
        //        {
        //            dbContext.DiscoConfiguration.Wireless.eduSTAR_ServiceAccountUsername = Username;
        //            dbContext.SaveChanges();
        //            return Json("OK", JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            throw new Exception("The Username cannot be null or empty");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
        //    }
        //}
        //public virtual ActionResult eduSTAR_Password(string Password)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrWhiteSpace(Password))
        //        {
        //            dbContext.DiscoConfiguration.Wireless.eduSTAR_ServiceAccountPassword = Password;
        //            dbContext.SaveChanges();
        //            return Json("OK", JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            throw new Exception("The Password cannot be null or empty");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
        //    }
        //}

        //#endregion

    }
}
