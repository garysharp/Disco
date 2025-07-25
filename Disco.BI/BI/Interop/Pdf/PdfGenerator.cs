using Disco.BI.Extensions;
using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using Disco.Models.Services.Expressions.Extensions;
using Disco.Services;
using Disco.Services.Documents;
using Disco.Services.Expressions;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Tasks;
using Disco.Services.Users;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.codec;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Disco.BI.Interop.Pdf
{
    public static class PdfGenerator
    {
        public static Stream GenerateBulkFromPackage(DocumentTemplatePackage package, DiscoDataContext database, User creatorUser, DateTime timestamp, bool? insertBlankPages, List<IAttachmentTarget> dataObjects)
        {
            if (dataObjects.Count > 0)
            {
                List<Stream> generatedPdfs = new List<Stream>(dataObjects.Count);
                using (var state = DocumentState.DefaultState())
                {
                    foreach (var d in dataObjects)
                    {
                        generatedPdfs.Add(package.GeneratePdfPackage(database, d, creatorUser, timestamp, state));
                        state.SequenceNumber++;
                        state.FlushScopeCache();
                    }
                }
                if (generatedPdfs.Count == 1)
                {
                    return generatedPdfs[0];
                }
                else
                {
                    Stream bulkPdf = Utilities.JoinPdfs(insertBlankPages.GetValueOrDefault(package.InsertBlankPages), generatedPdfs);
                    foreach (Stream singlePdf in generatedPdfs)
                        singlePdf.Dispose();
                    return bulkPdf;
                }
            }
            return null;
        }

        public static Stream GenerateBulkFromPackage(DocumentTemplatePackage package, DiscoDataContext database, User creatorUser, DateTime timestamp, bool? insertBlankPages, List<string> dataObjectsIds)
        {
            List<IAttachmentTarget> DataObjects;

            switch (package.Scope)
            {
                case AttachmentTypes.Device:
                    DataObjects = database.Devices.Where(d => dataObjectsIds.Contains(d.SerialNumber)).ToList<IAttachmentTarget>();
                    break;
                case AttachmentTypes.Job:
                    int[] intDataObjectsIds = dataObjectsIds.Select(i => int.Parse(i)).ToArray();
                    DataObjects = database.Jobs.Where(j => intDataObjectsIds.Contains(j.Id)).ToList<IAttachmentTarget>();
                    break;
                case AttachmentTypes.User:
                    DataObjects = new List<IAttachmentTarget>(dataObjectsIds.Count);
                    for (int idIndex = 0; idIndex < dataObjectsIds.Count; idIndex++)
                    {
                        string dataObjectId = dataObjectsIds[idIndex];
                        var user = UserService.GetUser(ActiveDirectory.ParseDomainAccountId(dataObjectId), database, true);
                        if (user == null)
                            throw new Exception($"Unknown Username specified: {dataObjectId}");
                        DataObjects.Add(user);
                    }
                    break;
                default:
                    throw new InvalidOperationException("Invalid DocumentType Scope");
            }

            return GenerateBulkFromPackage(package, database, creatorUser, timestamp, insertBlankPages, DataObjects);
        }

        public static Stream GenerateFromPackage(DocumentTemplatePackage package, DiscoDataContext database, IAttachmentTarget data, User creatorUser, DateTime timestamp, DocumentState state)
        {
            var templates = package.GetDocumentTemplates(database);

            if (templates.Count == 0)
                return null;

            bool generateExpression = !string.IsNullOrEmpty(package.OnGenerateExpression);
            string generateExpressionResult = null;

            if (generateExpression)
                generateExpressionResult = package.EvaluateOnGenerateExpression(data, database, creatorUser, timestamp, state);

            List<Stream> generatedPdfs = new List<Stream>(templates.Count);
            foreach (var template in templates)
            {
                generatedPdfs.Add(template.GeneratePdf(database, data, creatorUser, timestamp, state, true));

                state.SequenceNumber++;
                state.FlushScopeCache();
            }

            if (generateExpression)
                DocumentsLog.LogDocumentPackageGenerated(package, data, creatorUser, generateExpressionResult);
            else
                DocumentsLog.LogDocumentPackageGenerated(package, data, creatorUser);

            if (generatedPdfs.Count == 1)
            {
                return generatedPdfs[0];
            }
            else
            {
                Stream bulkPdf = Utilities.JoinPdfs(package.InsertBlankPages, generatedPdfs);
                foreach (Stream singlePdf in generatedPdfs)
                    singlePdf.Dispose();
                return bulkPdf;
            }
        }
        public static Stream GenerateBulkFromTemplate(DocumentTemplate dt, DiscoDataContext database, User creatorUser, DateTime timestamp, bool insertBlankPages, List<IAttachmentTarget> dataObjects, IScheduledTaskStatus taskStatus)
        {
            if (dataObjects.Count > 0)
            {
                List<Stream> generatedPdfs = new List<Stream>(dataObjects.Count);
                var progressPerDoc = 80d / dataObjects.Count;
                var progressDoc = 10d;
                using (var state = DocumentState.DefaultState())
                {
                    taskStatus.UpdateStatus(10, "Rendering", "Starting");
                    foreach (var d in dataObjects)
                    {
                        taskStatus.UpdateStatus(progressDoc += progressPerDoc, $"Rendering {d.AttachmentReferenceId}");
                        generatedPdfs.Add(dt.GeneratePdf(database, d, creatorUser, timestamp, state, true));
                        state.SequenceNumber++;
                        state.FlushScopeCache();
                    }
                }
                if (generatedPdfs.Count == 1)
                {
                    return generatedPdfs[0];
                }
                else
                {
                    taskStatus.UpdateStatus(90, "Merging", "Merging documents");
                    Stream bulkPdf = Utilities.JoinPdfs(insertBlankPages, generatedPdfs);
                    foreach (Stream singlePdf in generatedPdfs)
                        singlePdf.Dispose();
                    return bulkPdf;
                }
            }
            return null;
        }

        public static Stream GenerateBulkFromTemplate(DocumentTemplate dt, DiscoDataContext database, User creatorUser, DateTime timestamp, bool insertBlankPages, List<string> dataObjectsIds, IScheduledTaskStatus taskStatus)
        {
            Dictionary<string, IAttachmentTarget> dataObjectLookup;
            List<string> dataObjectIds = dataObjectsIds;

            taskStatus.UpdateStatus(0, "Resolving targets", "Resolving render targets");

            switch (dt.Scope)
            {
                case DocumentTemplate.DocumentTemplateScopes.Device:
                    dataObjectLookup = database.Devices.Where(d => dataObjectsIds.Contains(d.SerialNumber)).AsEnumerable().Cast<IAttachmentTarget>().ToDictionary(i => i.AttachmentReferenceId, StringComparer.OrdinalIgnoreCase);
                    break;
                case DocumentTemplate.DocumentTemplateScopes.Job:
                    var intDataObjectsIds = dataObjectsIds.Select(i => int.Parse(i)).ToList();
                    dataObjectLookup = database.Jobs.Where(j => intDataObjectsIds.Contains(j.Id)).AsEnumerable().Cast<IAttachmentTarget>().ToDictionary(i => i.AttachmentReferenceId, StringComparer.OrdinalIgnoreCase);
                    break;
                case DocumentTemplate.DocumentTemplateScopes.User:
                    dataObjectLookup = new Dictionary<string, IAttachmentTarget>(dataObjectsIds.Count, StringComparer.OrdinalIgnoreCase);
                    dataObjectIds = new List<string>(dataObjectsIds.Count);
                    foreach (var userId in dataObjectsIds)
                    {
                        var user = UserService.GetUser(ActiveDirectory.ParseDomainAccountId(userId), database, true);
                        if (user == null)
                        {
                            dataObjectIds.Add(userId);
                            continue;
                        }
                        dataObjectIds.Add(user.UserId);
                        dataObjectLookup.Add(user.UserId, user);
                    }
                    break;
                default:
                    throw new InvalidOperationException("Invalid DocumentType Scope");
            }

            // recreate list to honor the sort-order provided in DataObjectsIds
            var dataObjects = new List<IAttachmentTarget>(dataObjectsIds.Count);
            var missingIds = new List<string>();
            foreach (var id in dataObjectIds)
            {
                if (dataObjectLookup.TryGetValue(id, out var dataObject))
                    dataObjects.Add(dataObject);
                else
                    missingIds.Add(id);
            }
            if (missingIds.Any())
            {
                throw new Exception($"Unknown id specified: {string.Join("; ", missingIds)}");
            }

            return GenerateBulkFromTemplate(dt, database, creatorUser, timestamp, insertBlankPages, dataObjects, taskStatus);
        }

        public static Stream GenerateFromTemplate(DocumentTemplate dt, DiscoDataContext Database, IAttachmentTarget Data, User CreatorUser, DateTime TimeStamp, DocumentState State, bool flattenFields = false)
        {
            // Validate Data
            switch (dt.Scope)
            {
                case DocumentTemplate.DocumentTemplateScopes.Device:
                    if (!(Data is Device))
                        throw new ArgumentException("This AttachmentType is configured for Devices only", "Data");
                    break;
                case DocumentTemplate.DocumentTemplateScopes.Job:
                    if (!(Data is Job))
                        throw new ArgumentException("This AttachmentType is configured for Jobs only", "Data");
                    break;
                case DocumentTemplate.DocumentTemplateScopes.User:
                    if (!(Data is User))
                        throw new ArgumentException("This AttachmentType is configured for Users only", "Data");
                    break;
                default:
                    throw new InvalidOperationException("Invalid AttachmentType Scope");
            }

            Database.Configuration.LazyLoadingEnabled = true;

            // Override FlattenFields if Document Template instructs.
            if (dt.FlattenForm)
                flattenFields = true;

            var expressionCache = dt.PdfExpressionsFromCache(Database);

            string templateFilename = dt.RepositoryFilename(Database);
            PdfReader pdfReader = new PdfReader(templateFilename);

            MemoryStream pdfGeneratedStream = new MemoryStream();
            PdfStamper pdfStamper = new PdfStamper(pdfReader, pdfGeneratedStream);

            pdfStamper.FormFlattening = flattenFields;
            pdfStamper.Writer.CloseStream = false;

            IDictionary expressionVariables = Expression.StandardVariables(dt, Database, CreatorUser, TimeStamp, State, Data);

            foreach (string pdfFieldKey in pdfStamper.AcroFields.Fields.Keys)
            {
                if (pdfFieldKey.Equals("DiscoAttachmentId", StringComparison.OrdinalIgnoreCase))
                {
                    AcroFields.Item fields = pdfStamper.AcroFields.Fields[pdfFieldKey];
                    string fieldValue = dt.CreateUniqueIdentifier(Database, Data, CreatorUser, TimeStamp, 0).ToJson();
                    if (flattenFields)
                        pdfStamper.AcroFields.SetField(pdfFieldKey, string.Empty);
                    else
                        pdfStamper.AcroFields.SetField(pdfFieldKey, fieldValue);

                    IList<AcroFields.FieldPosition> pdfFieldPositions = pdfStamper.AcroFields.GetFieldPositions(pdfFieldKey);
                    for (int pdfFieldOrdinal = 0; pdfFieldOrdinal < fields.Size; pdfFieldOrdinal++)
                    {
                        AcroFields.FieldPosition pdfFieldPosition = pdfFieldPositions[pdfFieldOrdinal];

                        // Create Binary Unique Identifier
                        var pageUniqueId = dt.CreateUniqueIdentifier(Database, Data, CreatorUser, TimeStamp, pdfFieldPosition.page);
                        var pageUniqueIdBytes = pageUniqueId.ToQRCodeBytes();

                        // Encode to QRCode byte array
                        var pageUniqueIdEncoded = QRCodeBinaryEncoder.Encode(pageUniqueIdBytes, out var qrWidth, out var qrHeight);

                        // Encode byte array to Image
                        var pageUniqueIdImageData = CCITTG4Encoder.Compress(pageUniqueIdEncoded, qrWidth, qrHeight);
                        var pageUniqueIdImage = iTextSharp.text.Image.GetInstance(qrWidth, qrHeight, false, 256, 1, pageUniqueIdImageData, null);

                        // Add to the pdf page
                        pageUniqueIdImage.SetAbsolutePosition(pdfFieldPosition.position.Left, pdfFieldPosition.position.Bottom);
                        pageUniqueIdImage.ScaleToFit(pdfFieldPosition.position.Width, pdfFieldPosition.position.Height);
                        pdfStamper.GetOverContent(pdfFieldPosition.page).AddImage(pageUniqueIdImage);
                    }
                    // Hide Fields
                    PdfDictionary field = fields.GetValue(0);
                    if ((PdfName)field.Get(PdfName.TYPE) == PdfName.ANNOT)
                    {
                        field.Put(PdfName.F, new PdfNumber(6));
                    }
                    else
                    {
                        PdfArray fieldKids = (PdfArray)field.Get(PdfName.KIDS);
                        foreach (PdfIndirectReference fieldKidRef in fieldKids)
                        {
                            ((PdfDictionary)pdfReader.GetPdfObject(fieldKidRef.Number)).Put(PdfName.F, new PdfNumber(6));
                        }
                    }
                }
                else
                {
                    if (expressionCache.TryGetValue(pdfFieldKey, out var fieldExpression))
                    {
                        if (fieldExpression.IsDynamic)
                        {
                            Tuple<string, bool, object> fieldExpressionResult = fieldExpression.Evaluate(Data, expressionVariables);

                            if (fieldExpressionResult.Item3 != null)
                            {
                                IImageExpressionResult imageResult = (fieldExpressionResult.Item3 as IImageExpressionResult);
                                if (imageResult != null)
                                {
                                    // Output Image
                                    AcroFields.Item fields = pdfStamper.AcroFields.Fields[pdfFieldKey];
                                    IList<AcroFields.FieldPosition> pdfFieldPositions = pdfStamper.AcroFields.GetFieldPositions(pdfFieldKey);
                                    for (int pdfFieldOrdinal = 0; pdfFieldOrdinal < fields.Size; pdfFieldOrdinal++)
                                    {
                                        AcroFields.FieldPosition pdfFieldPosition = pdfFieldPositions[pdfFieldOrdinal];

                                        iTextSharp.text.Image pdfImage;
                                        var imageWidth = (int)(pdfFieldPosition.position.Width * 1.6);
                                        var imageHeight = (int)(pdfFieldPosition.position.Height * 1.6);
                                        if (imageResult.Format == ImageExpressionFormat.Jpeg || imageResult.Format == ImageExpressionFormat.Png)
                                        {
                                            pdfImage = iTextSharp.text.Image.GetInstance(imageResult.GetImage(imageWidth, imageHeight));
                                        }
                                        else if (imageResult.Format == ImageExpressionFormat.CcittG4)
                                        {
                                            var imageData = imageResult.GetImage(out imageWidth, out imageHeight);
                                            pdfImage = iTextSharp.text.Image.GetInstance(imageWidth, imageHeight, false, 256, 1, imageData.GetBuffer(), null);
                                        }
                                        else
                                            throw new NotSupportedException($"Unexpected image format {imageResult.Format}");

                                        pdfImage.SetAbsolutePosition(pdfFieldPosition.position.Left, pdfFieldPosition.position.Bottom);
                                        pdfImage.ScaleToFit(pdfFieldPosition.position.Width, pdfFieldPosition.position.Height);
                                        pdfStamper.GetOverContent(pdfFieldPosition.page).AddImage(pdfImage);
                                    }
                                    if (!fieldExpressionResult.Item2 && !imageResult.ShowField)
                                    {
                                        // Hide Fields
                                        PdfDictionary field = fields.GetValue(0);
                                        if ((PdfName)field.Get(PdfName.TYPE) == PdfName.ANNOT)
                                        {
                                            field.Put(PdfName.F, new PdfNumber(6));
                                        }
                                        else
                                        {
                                            PdfArray fieldKids = (PdfArray)field.Get(PdfName.KIDS);
                                            foreach (PdfIndirectReference fieldKidRef in fieldKids)
                                            {
                                                ((PdfDictionary)pdfReader.GetPdfObject(fieldKidRef.Number)).Put(PdfName.F, new PdfNumber(6));
                                            }
                                        }
                                    }
                                }
                            }

                            pdfStamper.AcroFields.SetField(pdfFieldKey, fieldExpressionResult.Item1);

                            if (fieldExpressionResult.Item2) // Expression Error
                            {
                                AcroFields.Item fields = pdfStamper.AcroFields.Fields[pdfFieldKey];
                                for (int pdfFieldOrdinal = 0; pdfFieldOrdinal < fields.Size; pdfFieldOrdinal++)
                                {
                                    PdfDictionary field = fields.GetValue(pdfFieldOrdinal);
                                    PdfDictionary fieldMK;
                                    if (field.Contains(PdfName.MK))
                                        fieldMK = field.GetAsDict(PdfName.MK);
                                    else
                                    {
                                        fieldMK = new PdfDictionary(PdfName.MK);
                                        field.Put(PdfName.MK, fieldMK);
                                    }
                                    fieldMK.Put(PdfName.BC, new PdfArray(new float[] { 1, 0, 0 }));
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Pdf template field expressions are out of sync with the expression cache");
                    }
                }
                State.FlushFieldCache();
            }

            pdfStamper.Close();
            pdfReader.Close();

            if (dt.Scope == DocumentTemplate.DocumentTemplateScopes.Job)
            {
                // Write Job Log

                Job j = (Job)Data;
                JobLog jl = new JobLog()
                {
                    JobId = j.Id,
                    TechUserId = CreatorUser.UserId,
                    Timestamp = DateTime.Now
                };
                jl.Comments = $"# Document Generated\r\n**{dt.Description}** [{dt.Id}]";
                Database.JobLogs.Add(jl);
            }

            pdfGeneratedStream.Position = 0;
            return pdfGeneratedStream;
        }

    }
}
