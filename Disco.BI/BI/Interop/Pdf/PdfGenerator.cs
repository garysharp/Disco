using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;
using Disco.Data.Repository;
using Disco.Models.BI.DocumentTemplates;
using System.IO;
using iTextSharp.text.pdf;
using System.Collections.Concurrent;
using Disco.BI.Expressions;
using System.Collections;
using Disco.BI.Extensions;
using Disco.Models.BI.Expressions;
using Disco.Services.Users;

namespace Disco.BI.Interop.Pdf
{
    public static class PdfGenerator
    {

        public static System.IO.Stream GenerateBulkFromTemplate(DocumentTemplate dt, DiscoDataContext Database, User CreatorUser, System.DateTime Timestamp, params object[] DataObjects)
        {
            if (DataObjects.Length > 0)
            {
                List<Stream> generatedPdfs = new List<Stream>(DataObjects.Length);
                using (Models.BI.DocumentTemplates.DocumentState state = Models.BI.DocumentTemplates.DocumentState.DefaultState())
                {
                    foreach (object d in DataObjects)
                    {
                        generatedPdfs.Add(dt.GeneratePdf(Database, d, CreatorUser, Timestamp, state, true));
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
                    Stream bulkPdf = DocumentTemplateBI.Utilities.JoinPdfs(generatedPdfs.ToArray());
                    foreach (Stream singlePdf in generatedPdfs)
                        singlePdf.Dispose();
                    return bulkPdf;
                }
            }
            return null;
        }

        public static System.IO.Stream GenerateBulkFromTemplate(DocumentTemplate dt, DiscoDataContext Database, User CreatorUser, System.DateTime Timestamp, params string[] DataObjectsIds)
        {
            object[] DataObjects;

            switch (dt.Scope)
            {
                case DocumentTemplate.DocumentTemplateScopes.Device:
                    DataObjects = Database.Devices.Where(d => DataObjectsIds.Contains(d.SerialNumber)).ToArray();
                    break;
                case DocumentTemplate.DocumentTemplateScopes.Job:
                    int[] intDataObjectsIds = DataObjectsIds.Select(i => int.Parse(i)).ToArray();
                    DataObjects = Database.Jobs.Where(j => intDataObjectsIds.Contains(j.Id)).ToArray();
                    break;
                case DocumentTemplate.DocumentTemplateScopes.User:
                    DataObjects = new object[DataObjectsIds.Length];
                    for (int idIndex = 0; idIndex < DataObjectsIds.Length; idIndex++)
                    {
                        string dataObjectId = DataObjectsIds[idIndex];
                        if (!dataObjectId.Contains('\\'))
                            dataObjectId = Disco.Services.Interop.ActiveDirectory.ActiveDirectory.PrimaryDomain.NetBiosName + @"\" + dataObjectId;

                        DataObjects[idIndex] = UserService.GetUser(DataObjectsIds[idIndex], Database, true);
                        if (DataObjects[idIndex] == null)
                            throw new Exception(string.Format("Unknown Username specified: {0}", DataObjectsIds[idIndex]));
                    }
                    break;
                default:
                    throw new InvalidOperationException("Invalid DocumentType Scope");
            }

            return GenerateBulkFromTemplate(dt, Database, CreatorUser, Timestamp, DataObjects);
        }

        public static System.IO.Stream GenerateFromTemplate(DocumentTemplate dt, DiscoDataContext Database, object Data, User CreatorUser, System.DateTime TimeStamp, DocumentState State, bool FlattenFields = false)
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
                FlattenFields = true;

            ConcurrentDictionary<string, Expression> expressionCache = dt.PdfExpressionsFromCache(Database);

            string templateFilename = dt.RepositoryFilename(Database);
            PdfReader pdfReader = new PdfReader(templateFilename);

            MemoryStream pdfGeneratedStream = new MemoryStream();
            PdfStamper pdfStamper = new PdfStamper(pdfReader, pdfGeneratedStream);

            pdfStamper.FormFlattening = FlattenFields;
            pdfStamper.Writer.CloseStream = false;

            IDictionary expressionVariables = Expression.StandardVariables(dt, Database, CreatorUser, TimeStamp, State);

            foreach (string pdfFieldKey in pdfStamper.AcroFields.Fields.Keys)
            {
                if (pdfFieldKey.Equals("DiscoAttachmentId", StringComparison.InvariantCultureIgnoreCase))
                {
                    AcroFields.Item fields = pdfStamper.AcroFields.Fields[pdfFieldKey];
                    string fieldValue = dt.UniqueIdentifier(Data, CreatorUser.UserId, TimeStamp);
                    if (FlattenFields)
                        pdfStamper.AcroFields.SetField(pdfFieldKey, String.Empty);
                    else
                        pdfStamper.AcroFields.SetField(pdfFieldKey, fieldValue);

                    IList<AcroFields.FieldPosition> pdfFieldPositions = pdfStamper.AcroFields.GetFieldPositions(pdfFieldKey);
                    for (int pdfFieldOrdinal = 0; pdfFieldOrdinal < fields.Size; pdfFieldOrdinal++)
                    {
                        AcroFields.FieldPosition pdfFieldPosition = pdfFieldPositions[pdfFieldOrdinal];
                        string pdfBarcodeContent = dt.UniquePageIdentifier(Data, CreatorUser.UserId, TimeStamp, pdfFieldPosition.page);
                        BarcodeQRCode pdfBarcode = new BarcodeQRCode(pdfBarcodeContent, (int)pdfFieldPosition.position.Width, (int)pdfFieldPosition.position.Height, null);
                        iTextSharp.text.Image pdfBarcodeImage = pdfBarcode.GetImage();
                        pdfBarcodeImage.SetAbsolutePosition(pdfFieldPosition.position.Left, pdfFieldPosition.position.Bottom);
                        pdfStamper.GetOverContent(pdfFieldPosition.page).AddImage(pdfBarcodeImage);
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
                    Expression fieldExpression = null;
                    if (expressionCache.TryGetValue(pdfFieldKey, out fieldExpression))
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
                                        iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(imageResult.GetImage((int)pdfFieldPosition.position.Width, (int)pdfFieldPosition.position.Height));
                                        pdfImage.SetAbsolutePosition(pdfFieldPosition.position.Left, pdfFieldPosition.position.Bottom);
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
                jl.Comments = string.Format("Document Generated{0}{1} [{2}]", Environment.NewLine, dt.Description, dt.Id);
                Database.JobLogs.Add(jl);
            }

            pdfGeneratedStream.Position = 0;
            return pdfGeneratedStream;
        }

    }
}
