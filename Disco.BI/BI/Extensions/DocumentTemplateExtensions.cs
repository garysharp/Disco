using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Web;
using Disco.Data.Repository;
using Disco.Models.BI.DocumentTemplates;
using Disco.Models.Repository;
using System.Collections;
using System.Collections.Generic;
using iTextSharp.text.pdf;
using Disco.BI.Expressions;
using System.Drawing;
using System.IO;
using Disco.BI.DocumentTemplateBI;

namespace Disco.BI.Extensions
{
    public static class DocumentTemplateExtensions
    {
        private const string DocumentTemplateExpressionCacheTemplate = "DocumentTemplate_{0}";

        public static string RepositoryFilename(this DocumentTemplate dt, DiscoDataContext Database)
        {
            return System.IO.Path.Combine(DataStore.CreateLocation(Database, "DocumentTemplates"), string.Format("{0}.pdf", dt.Id));
        }
        public static string SavePdfTemplate(this DocumentTemplate dt, DiscoDataContext Database, Stream TemplateFile)
        {
            string filePath = dt.RepositoryFilename(Database);
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                TemplateFile.CopyTo(fs);
            }
            Expressions.ExpressionCache.InvalidModule(string.Format(DocumentTemplateExpressionCacheTemplate, dt.Id));
            return filePath;
        }

        public static DisposableImageCollection PdfPageImages(this PdfReader pdfReader, int PageNumber)
        {
            return Interop.Pdf.PdfImporter.GetPageImages(pdfReader, PageNumber);
        }

        public static ConcurrentDictionary<string, Expression> PdfExpressionsFromCache(this DocumentTemplate dt, DiscoDataContext Database)
        {
            string cacheModuleKey = string.Format(DocumentTemplateExpressionCacheTemplate, dt.Id);
            var module = Expressions.ExpressionCache.GetModule(cacheModuleKey);
            if (module == null)
            {
                // Cache
                string templateFilename = dt.RepositoryFilename(Database);
                PdfReader pdfReader = new PdfReader(templateFilename);
                int pdfFieldOrdinal = 0;
                foreach (string pdfFieldKey in pdfReader.AcroFields.Fields.Keys)
                {
                    var pdfFieldValue = pdfReader.AcroFields.GetField(pdfFieldKey);
                    Expressions.ExpressionCache.SetValue(cacheModuleKey, pdfFieldKey, Expressions.Expression.Tokenize(pdfFieldKey, pdfFieldValue, pdfFieldOrdinal));
                    pdfFieldOrdinal++;
                }
                pdfReader.Close();
                module = Expressions.ExpressionCache.GetModule(cacheModuleKey, true);
            }
            return module;
        }

        public static List<BI.Expressions.Expression> ExtractPdfExpressions(this DocumentTemplate dt, DiscoDataContext Database)
        {
            return dt.PdfExpressionsFromCache(Database).Values.OrderBy(e => e.Ordinal).ToList();
        }
        public static System.IO.Stream GeneratePdfBulk(this DocumentTemplate dt, DiscoDataContext Database, User CreatorUser, System.DateTime Timestamp, params string[] DataObjectsIds)
        {
            return Interop.Pdf.PdfGenerator.GenerateBulkFromTemplate(dt, Database, CreatorUser, Timestamp, DataObjectsIds);
        }
        public static System.IO.Stream GeneratePdfBulk(this DocumentTemplate dt, DiscoDataContext Database, User CreatorUser, System.DateTime Timestamp, params object[] DataObjects)
        {
            return Interop.Pdf.PdfGenerator.GenerateBulkFromTemplate(dt, Database, CreatorUser, Timestamp, DataObjects);
        }
        public static System.IO.Stream GeneratePdf(this DocumentTemplate dt, DiscoDataContext Database, object Data, User CreatorUser, System.DateTime TimeStamp, DocumentState State, bool FlattenFields = false)
        {
            return Interop.Pdf.PdfGenerator.GenerateFromTemplate(dt, Database, Data, CreatorUser, TimeStamp, State, FlattenFields);
        }

        public static Expression FilterExpressionFromCache(this DocumentTemplate dt)
        {
            return ExpressionCache.GetValue("DocumentTemplateFilterExpression", dt.Id, () => { return Expression.TokenizeSingleDynamic(null, dt.FilterExpression, 0); });
        }
        public static void FilterExpressionInvalidateCache(this DocumentTemplate dt)
        {
            ExpressionCache.InvalidateKey("DocumentTemplateFilterExpression", dt.Id);
        }
        public static bool FilterExpressionMatches(this DocumentTemplate dt, object Data, DiscoDataContext Database, User User, System.DateTime TimeStamp, DocumentState State)
        {
            if (!string.IsNullOrEmpty(dt.FilterExpression))
            {
                Expression compiledExpression = dt.FilterExpressionFromCache();
                System.Collections.IDictionary evaluatorVariables = Expression.StandardVariables(dt, Database, User, TimeStamp, State);
                try
                {
                    object er = compiledExpression.EvaluateFirst<object>(Data, evaluatorVariables);
                    if (er is bool)
                    {
                        return (bool)er;
                    }
                    bool erBool;
                    if (bool.TryParse(er.ToString(), out erBool))
                    {
                        return erBool;
                    }
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
        public static string GetDataId(this DocumentTemplate dt, object Data)
        {
            if (Data is string)
            {
                return (string)Data;
            }
            else
            {
                switch (dt.Scope)
                {
                    case Models.Repository.DocumentTemplate.DocumentTemplateScopes.Device:
                        if (!(Data is Device))
                            throw new ArgumentException("This Document Template is configured for Devices only", "Data");
                        Device d = (Device)Data;
                        return d.SerialNumber;
                    case Models.Repository.DocumentTemplate.DocumentTemplateScopes.Job:
                        if (!(Data is Job))
                            throw new ArgumentException("This Document Template is configured for Jobs only", "Data");
                        Job d2 = (Job)Data;
                        return d2.Id.ToString();
                    case Models.Repository.DocumentTemplate.DocumentTemplateScopes.User:
                        if (!(Data is User))
                            throw new ArgumentException("This Document Template is configured for Users only", "Data");
                        User d3 = (User)Data;
                        return d3.UserId;
                    default:
                        throw new InvalidOperationException("Invalid Document Template Scope");
                }
            }
        }
        public static string UniqueIdentifier(string DocumentTemplateId, string DataId, string CreatorId, System.DateTime Timestamp)
        {
            return string.Format("Disco|1|{0}|{1}|{2}|{3:s}",
                DocumentTemplateId,
                DataId,
                CreatorId,
                Timestamp
            );
        }
        public static string UniqueIdentifier(this DocumentTemplate dt, object Data, string CreatorId, System.DateTime Timestamp)
        {
            return string.Format("Disco|1|{0}|{1}|{2}|{3:s}",
                dt.Id,
                dt.GetDataId(System.Runtime.CompilerServices.RuntimeHelpers.GetObjectValue(Data)),
                CreatorId,
                Timestamp
            );
        }
        public static string UniquePageIdentifier(this DocumentTemplate dt, object Data, string CreatorId, System.DateTime Timestamp, int Page)
        {
            return string.Format("Disco|1|{0}|{1}|{2}|{3:s}|{4}",
                dt.Id,
                dt.GetDataId(System.Runtime.CompilerServices.RuntimeHelpers.GetObjectValue(Data)),
                CreatorId,
                Timestamp,
                Page
            );
        }
        public static List<RectangleF> QRCodeLocations(this DocumentTemplate dt, DiscoDataContext Database)
        {
            return DocumentTemplateBI.DocumentTemplateQRCodeLocationCache.GetLocations(dt, Database);
        }
        public static void Delete(this DocumentTemplate dt, DiscoDataContext Database)
        {
            // Find & Rename all references
            foreach (DeviceAttachment a in Database.DeviceAttachments.Where(a => a.DocumentTemplateId == dt.Id))
            {
                a.Comments = string.Format("{0} - {1}", dt.Description, a.Comments);
                if (a.Comments.Length > 500)
                    a.Comments = a.Comments.Substring(0, 500);
                a.DocumentTemplateId = null;
                a.DocumentTemplate = null;
            }
            foreach (JobAttachment a in Database.JobAttachments.Where(a => a.DocumentTemplateId == dt.Id))
            {
                a.Comments = string.Format("{0} - {1}", dt.Description, a.Comments);
                if (a.Comments.Length > 500)
                    a.Comments = a.Comments.Substring(0, 500);
                a.DocumentTemplateId = null;
                a.DocumentTemplate = null;
            }
            foreach (UserAttachment a in Database.UserAttachments.Where(a => a.DocumentTemplateId == dt.Id))
            {
                a.Comments = string.Format("{0} - {1}", dt.Description, a.Comments);
                if (a.Comments.Length > 500)
                    a.Comments = a.Comments.Substring(0, 500);
                a.DocumentTemplateId = null;
                a.DocumentTemplate = null;
            }

            // Delete SubTypes
            dt.JobSubTypes.Clear();

            // Delete Template
            string templateRepositoryFilename = dt.RepositoryFilename(Database);
            if (System.IO.File.Exists(templateRepositoryFilename))
                System.IO.File.Delete(templateRepositoryFilename);

            // Remove from Cache
            dt.FilterExpressionInvalidateCache();

            // Delete Document Template from Repository
            Database.DocumentTemplates.Remove(dt);
        }
    }
}
