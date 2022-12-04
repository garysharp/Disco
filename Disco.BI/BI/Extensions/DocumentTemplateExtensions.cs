using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using Disco.Services;
using Disco.Services.Documents;
using Disco.Services.Documents.ManagedGroups;
using Disco.Services.Expressions;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Tasks;
using iTextSharp.text.pdf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Drawing;
using System.IO;
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
                    var pdfField = pdfReader.AcroFields.Fields[pdfFieldKey];
                    var pdfFieldPositions = pdfReader.AcroFields.GetFieldPositions(pdfFieldKey);
                    var pdfFieldFlags = pdfField.GetMerged(0).GetAsNumber(PdfName.FF)?.IntValue ?? 0;
                    var isRequired = (pdfFieldFlags & 2) == 2;
                    var isReadOnly = (pdfFieldFlags & 1) == 1;

                    var pdfFieldValue = pdfReader.AcroFields.GetField(pdfFieldKey);
                    var pdfFieldPosition = default(RectangleF?);
                    if (pdfFieldPositions != null && pdfFieldPositions.Count > 0)
                    {
                        var position = pdfFieldPositions.First().position;
                        pdfFieldPosition = new RectangleF(position.Left, position.Top, position.Width, position.Height);
                    }

                    ExpressionCache.SetValue(cacheModuleKey, pdfFieldKey, Expression.Tokenize(pdfFieldKey, pdfFieldValue, pdfFieldOrdinal, isRequired, isReadOnly, pdfFieldPosition));
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

        public static Stream GeneratePdf(this DocumentTemplate dt, DiscoDataContext Database, IAttachmentTarget Target, User CreatorUser, DateTime TimeStamp, DocumentState State, bool FlattenFields = false)
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
        public static Stream GeneratePdfPackage(this DocumentTemplatePackage package, DiscoDataContext Database, IAttachmentTarget Target, User CreatorUser, DateTime TimeStamp, DocumentState State)
        {
            return Interop.Pdf.PdfGenerator.GenerateFromPackage(package, Database, Target, CreatorUser, TimeStamp, State);
        }
        public static Stream GeneratePdfPackageBulk(this DocumentTemplatePackage package, DiscoDataContext Database, User CreatorUser, DateTime Timestamp, bool InsertBlankPages, List<string> DataObjectsIds)
        {
            return Interop.Pdf.PdfGenerator.GenerateBulkFromPackage(package, Database, CreatorUser, Timestamp, InsertBlankPages, DataObjectsIds);
        }
        public static Stream GeneratePdfPackageBulk(this DocumentTemplatePackage package, DiscoDataContext Database, User CreatorUser, DateTime Timestamp, bool InsertBlankPages, List<IAttachmentTarget> DataObjects)
        {
            return Interop.Pdf.PdfGenerator.GenerateBulkFromPackage(package, Database, CreatorUser, Timestamp, InsertBlankPages, DataObjects);
        }

        public static List<bool> PdfPageHasAttachmentId(this DocumentTemplate dt, DiscoDataContext Database)
        {
            string templateFilename = dt.RepositoryFilename(Database);
            if (!File.Exists(templateFilename))
                throw new FileNotFoundException("PDF template not found", templateFilename);

            PdfReader pdfReader = new PdfReader(templateFilename);
            var result = new bool[pdfReader.NumberOfPages];
            var fieldNames = pdfReader.AcroFields.Fields.Keys.Where(key => key.Equals("DiscoAttachmentId", StringComparison.OrdinalIgnoreCase)).ToList();
            var fieldPositions = fieldNames.SelectMany(name => pdfReader.AcroFields.GetFieldPositions(name));
            foreach (var fieldPosition in fieldPositions)
            {
                result[fieldPosition.page - 1] = true;
            }
            pdfReader.Close();
            return result.ToList();
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
