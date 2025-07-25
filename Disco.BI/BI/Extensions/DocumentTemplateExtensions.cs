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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Disco.BI.Extensions
{
    public static class DocumentTemplateExtensions
    {
        public static Tuple<Dictionary<string, Expression>, List<DocumentField>> CreateExpressions(string templateFileName, DiscoDataContext database)
        {
            Dictionary<string, Expression> expressions = new Dictionary<string, Expression>();
            List<DocumentField> fields = new List<DocumentField>();

            PdfReader pdfReader = new PdfReader(templateFileName);
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
                var fieldTypeId = pdfReader.AcroFields.GetFieldType(pdfFieldKey);
                var fieldType = DocumentFieldType.None;
                if (fieldTypeId <= 8 && fieldTypeId > 0)
                    fieldType = (DocumentFieldType)fieldTypeId;

                var fixedValues = default(List<string>);

                if (fieldType == DocumentFieldType.RadioButton || fieldType == DocumentFieldType.Checkbox)
                {
                    fixedValues = pdfReader.AcroFields.GetAppearanceStates(pdfFieldKey).ToList();
                }

                expressions[pdfFieldKey] = Expression.Tokenize(pdfFieldKey, pdfFieldValue, pdfFieldOrdinal, isRequired, isReadOnly, pdfFieldPosition);
                fields.Add(new DocumentField(pdfFieldKey, pdfFieldValue, pdfFieldOrdinal, fieldType, isRequired, isReadOnly, fixedValues));
                pdfFieldOrdinal++;
            }
            pdfReader.Close();

            return Tuple.Create(expressions, fields);
        }

        private static Tuple<Dictionary<string, Expression>, List<DocumentField>> CreateExpressions(DocumentTemplate dt, DiscoDataContext database)
        {
            string templateFileName = dt.RepositoryFilename(database);
            return CreateExpressions(templateFileName, database);
        }

        public static Dictionary<string, Expression> PdfExpressionsFromCache(this DocumentTemplate dt, DiscoDataContext database)
        {
            return ExpressionCache.GetOrCreateExpressions(dt, () => CreateExpressions(dt, database));
        }

        public static List<DocumentField> PdfFieldsFromCache(this DocumentTemplate dt, DiscoDataContext database)
        {
            return ExpressionCache.GetOrCreateFields(dt, () => CreateExpressions(dt, database));
        }

        public static List<Expression> ExtractPdfExpressions(this DocumentTemplate dt, DiscoDataContext database)
        {
            return dt.PdfExpressionsFromCache(database).Values.OrderBy(e => e.Ordinal).ToList();
        }

        public static Stream GeneratePdf(this DocumentTemplate dt, DiscoDataContext database, IAttachmentTarget target, User creatorUser, DateTime timeStamp, DocumentState state, bool flattenFields = false)
        {
            bool generateExpression = !string.IsNullOrEmpty(dt.OnGenerateExpression);
            string generateExpressionResult = null;

            if (generateExpression)
                generateExpressionResult = dt.EvaluateOnGenerateExpression(target, database, creatorUser, timeStamp, state);

            var pdfStream = Interop.Pdf.PdfGenerator.GenerateFromTemplate(dt, database, target, creatorUser, timeStamp, state, flattenFields);

            if (generateExpression)
                DocumentsLog.LogDocumentGenerated(dt, target, creatorUser, generateExpressionResult);
            else
                DocumentsLog.LogDocumentGenerated(dt, target, creatorUser);

            return pdfStream;
        }
        public static Stream GeneratePdfPackage(this DocumentTemplatePackage package, DiscoDataContext database, IAttachmentTarget target, User creatorUser, DateTime timeStamp, DocumentState state)
        {
            return Interop.Pdf.PdfGenerator.GenerateFromPackage(package, database, target, creatorUser, timeStamp, state);
        }
        public static Stream GeneratePdfPackageBulk(this DocumentTemplatePackage package, DiscoDataContext database, User creatorUser, DateTime timestamp, bool? insertBlankPages, List<string> dataObjectsIds)
        {
            return Interop.Pdf.PdfGenerator.GenerateBulkFromPackage(package, database, creatorUser, timestamp, insertBlankPages, dataObjectsIds);
        }
        public static Stream GeneratePdfPackageBulk(this DocumentTemplatePackage package, DiscoDataContext database, User creatorUser, DateTime timestamp, bool? insertBlankPages, List<IAttachmentTarget> dataObjects)
        {
            return Interop.Pdf.PdfGenerator.GenerateBulkFromPackage(package, database, creatorUser, timestamp, insertBlankPages, dataObjects);
        }

        public static List<bool> PdfPageHasAttachmentId(this DocumentTemplate dt, DiscoDataContext database)
        {
            string templateFilename = dt.RepositoryFilename(database);
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
            void updateAttachment(IAttachment a)
            {
                var comments = $"{dt.Description} - {a.Comments}";
                if (comments.Length > 500)
                    comments = comments.Substring(0, 500);
                a.Comments = comments;
                a.DocumentTemplateId = null;
            }
            foreach (var a in Database.DeviceAttachments.Where(a => a.DocumentTemplateId == dt.Id))
                updateAttachment(a);
            foreach (var a in Database.JobAttachments.Where(a => a.DocumentTemplateId == dt.Id))
                updateAttachment(a);
            foreach (UserAttachment a in Database.UserAttachments.Where(a => a.DocumentTemplateId == dt.Id))
                updateAttachment(a);

            // Remove Linked Group
            ActiveDirectory.Context.ManagedGroups.Remove(DocumentTemplateDevicesManagedGroup.GetKey(dt));
            ActiveDirectory.Context.ManagedGroups.Remove(DocumentTemplateUsersManagedGroup.GetKey(dt));

            // Delete SubTypes
            dt.JobSubTypes.Clear();

            // Delete Template
            string templateRepositoryFilename = dt.RepositoryFilename(Database);
            if (File.Exists(templateRepositoryFilename))
                File.Delete(templateRepositoryFilename);

            // Remove from Cache
            dt.FilterExpressionInvalidateCache();

            // Delete Document Template from Repository
            Database.DocumentTemplates.Remove(dt);
        }
    }
}
