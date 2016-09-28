using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using Disco.Services;
using Disco.Services.Documents;
using Disco.Services.Documents.ManagedGroups;
using Disco.Services.Expressions;
using Disco.Services.Interop.ActiveDirectory;
using iTextSharp.text.pdf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Disco.BI.Extensions
{
    public static class DocumentTemplateExtensions
    {
        internal const string CacheTemplate = "DocumentTemplate_{0}";

        public static ConcurrentDictionary<string, Expression> PdfExpressionsFromCache(this DocumentTemplate dt, DiscoDataContext Database)
        {
            string cacheModuleKey = string.Format(CacheTemplate, dt.Id);
            var module = ExpressionCache.GetModule(cacheModuleKey);
            if (module == null)
            {
                // Cache
                string templateFilename = dt.RepositoryFilename(Database);
                PdfReader pdfReader = new PdfReader(templateFilename);
                int pdfFieldOrdinal = 0;
                foreach (string pdfFieldKey in pdfReader.AcroFields.Fields.Keys)
                {
                    var pdfFieldValue = pdfReader.AcroFields.GetField(pdfFieldKey);
                    ExpressionCache.SetValue(cacheModuleKey, pdfFieldKey, Expression.Tokenize(pdfFieldKey, pdfFieldValue, pdfFieldOrdinal));
                    pdfFieldOrdinal++;
                }
                pdfReader.Close();
                module = ExpressionCache.GetModule(cacheModuleKey, true);
            }
            return module;
        }

        public static List<Expression> ExtractPdfExpressions(this DocumentTemplate dt, DiscoDataContext Database)
        {
            return dt.PdfExpressionsFromCache(Database).Values.OrderBy(e => e.Ordinal).ToList();
        }

        public static System.IO.Stream GeneratePdfBulk(this DocumentTemplate dt, DiscoDataContext Database, User CreatorUser, DateTime Timestamp, params string[] DataObjectsIds)
        {
            return Interop.Pdf.PdfGenerator.GenerateBulkFromTemplate(dt, Database, CreatorUser, Timestamp, DataObjectsIds);
        }
        public static System.IO.Stream GeneratePdfBulk(this DocumentTemplate dt, DiscoDataContext Database, User CreatorUser, DateTime Timestamp, params IAttachmentTarget[] DataObjects)
        {
            return Interop.Pdf.PdfGenerator.GenerateBulkFromTemplate(dt, Database, CreatorUser, Timestamp, DataObjects);
        }
        public static System.IO.Stream GeneratePdf(this DocumentTemplate dt, DiscoDataContext Database, IAttachmentTarget Target, User CreatorUser, DateTime TimeStamp, DocumentState State, bool FlattenFields = false)
        {
            bool generateExpression = !string.IsNullOrEmpty(dt.OnGenerateExpression);
            string generateExpressionResult = null;

            if (generateExpression)
                generateExpressionResult = dt.EvaluateOnGenerateExpression(Target, Database, CreatorUser, TimeStamp, State);

            var pdfStream = Interop.Pdf.PdfGenerator.GenerateFromTemplate(dt, Database, Target, CreatorUser, TimeStamp, State, FlattenFields);

            if (generateExpression)
                DocumentsLog.LogDocumentGenerated(dt, Target, CreatorUser, generateExpressionResult);
            else
                DocumentsLog.LogDocumentGenerated(dt, Target, CreatorUser);

            return pdfStream;
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

            // Remove Linked Group
            ActiveDirectory.Context.ManagedGroups.Remove(DocumentTemplateDevicesManagedGroup.GetKey(dt));
            ActiveDirectory.Context.ManagedGroups.Remove(DocumentTemplateUsersManagedGroup.GetKey(dt));

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
