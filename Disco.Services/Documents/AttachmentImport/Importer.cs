using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Logging;
using Disco.Services.Users;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Disco.Services.Documents.AttachmentImport
{
    public static class Importer
    {
        public static void Import(DiscoDataContext Database, string SessionId, string Filename)
        {
            var dataStoreUnassignedLocation = DataStore.CreateLocation(Database, "DocumentDropBox_Unassigned");
            var dataStoreSessionPagesCacheLocation = DataStore.CreateLocation(Database, "Cache\\DocumentDropBox_SessionPages");
            var documentTemplates = Database.DocumentTemplates.ToArray();

            if (!File.Exists(Filename))
            {
                DocumentsLog.LogImportWarning(SessionId, string.Format("File not found: {0}", Filename));
                throw new FileNotFoundException("Document Not Found", Filename);
            }

            DocumentsLog.LogImportProgress(SessionId, 0, "Reading File");

            List<ImportPage> pages = null;
            List<ImportPage> assignedPages;
            double progressInterval;

            try
            {
                // Use Pdfium to Rasterize and Detect Pages
                using (var importFileStream = new FileStream(Filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    using (var pdfiumDocument = PdfiumViewer.PdfDocument.Load(importFileStream))
                    {
                        progressInterval = 70D / pdfiumDocument.PageCount;
                        pages = new List<ImportPage>(pdfiumDocument.PageCount);
                        assignedPages = new List<ImportPage>(pdfiumDocument.PageCount);

                        // Rasterize and Detect Pages
                        for (int pageIndex = 0; pageIndex < pdfiumDocument.PageCount; pageIndex++)
                        {
                            var pageNumber = pageIndex + 1;

                            DocumentsLog.LogImportProgress(SessionId, (int)(pageIndex * progressInterval), $"Processing Page {pageNumber} of {pdfiumDocument.PageCount}");
                            DocumentsLog.LogImportPageStarting(SessionId, pageNumber);

                            var page = new ImportPage(Database, SessionId, pdfiumDocument, pageIndex);
                            pages.Add(page);

                            // Write Session Thumbnail
                            page.WriteThumbnailSessionCache();
                            DocumentsLog.LogImportPageImageUpdate(SessionId, pageNumber);

                            // Detect Image
                            if (page.DetectQRCode())
                            {
                                // Write updated session thumbnail
                                page.WriteThumbnailSessionCache();
                                DocumentsLog.LogImportPageImageUpdate(SessionId, pageNumber);
                                var identifier = page.Identifier;
                                DocumentsLog.LogImportPageDetected(SessionId, pageNumber, identifier.DocumentTemplate.Id, identifier.DocumentTemplate.Description, identifier.DocumentTemplate.Scope, identifier.Target.AttachmentReferenceId, identifier.Target.ToString());
                            }
                            else
                            {
                                page.WriteUndetectedImages();
                                DocumentsLog.LogImportPageUndetected(SessionId, pageNumber);
                            }
                        }
                    }
                }


                // Use PdfSharp to Import and Build Documents
                using (var importFileStream = new FileStream(Filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    using (var pdfSharpDocument = PdfReader.Open(importFileStream, PdfDocumentOpenMode.Import))
                    {
                        // Assign Pages
                        var documents = pages
                            .Where(p => p.IsValidAttachment)
                            .GroupBy(p => p.Identifier.DocumentGroupingId)
                            .ToList();

                        if (documents.Count > 0)
                        {
                            progressInterval = 20D / documents.Count;

                            foreach (var document in documents)
                            {
                                var documentPages = document.OrderBy(p => p.Identifier.PageIndex).ToList();
                                var documentPageFirst = documentPages.First();

                                DocumentsLog.LogImportProgress(SessionId, (int)(70D + (documents.IndexOf(document) * progressInterval)), $"Importing Documents {documents.IndexOf(document) + 1} of {documents.Count}");

                                using (MemoryStream msBuilder = new MemoryStream())
                                {
                                    using (var pdfSharpDocumentOutput = new PdfDocument())
                                    {
                                        foreach (var documentPage in documentPages)
                                        {
                                            var pdfSharpImportPage = pdfSharpDocument.Pages[documentPage.PageIndex];
                                            var importedPage = pdfSharpDocumentOutput.AddPage(pdfSharpImportPage);

                                            importedPage.Rotate = documentPage.Rotation;
                                        }

                                        pdfSharpDocumentOutput.Save(msBuilder, false);
                                    }

                                    msBuilder.Position = 0;
                                    using (var attachmentThumbnail = documentPageFirst.GetAttachmentThumbnail())
                                    {
                                        documentPageFirst.Identifier.ImportPdfAttachment(Database, msBuilder, attachmentThumbnail);
                                    }
                                }
                            }
                        }

                        // Write Unassigned Pages
                        var unassignedPages = pages
                            .Where(p => !p.IsValidAttachment)
                            .ToList();

                        if (unassignedPages.Count > 0)
                        {
                            progressInterval = 10D / unassignedPages.Count;

                            foreach (var documentPage in unassignedPages)
                            {
                                DocumentsLog.LogImportProgress(SessionId, (int)(90 + (unassignedPages.IndexOf(documentPage) * progressInterval)), string.Format("Processing Undetected Pages {0} of {1}", unassignedPages.IndexOf(documentPage) + 1, unassignedPages.Count));

                                using (var pdfSharpDocumentOutput = new PdfDocument())
                                {
                                    var pdfSharpImportPage = pdfSharpDocument.Pages[documentPage.PageIndex];
                                    pdfSharpDocumentOutput.AddPage(pdfSharpImportPage);

                                    var filename = Path.Combine(dataStoreUnassignedLocation, $"{SessionId}_{documentPage.PageIndex + 1}.pdf");

                                    pdfSharpDocumentOutput.Save(filename);

                                    DocumentsLog.LogImportPageUndetectedStored(SessionId, documentPage.PageIndex + 1);
                                }
                            }
                        }

                    }
                }
            }
            finally
            {
                // Dispose of pages
                if (pages != null && pages.Count != 0)
                {
                    for (int i = 0; i < pages.Count; i++)
                    {
                        pages[i].Dispose();
                    }
                }
            }
        }

        public static bool ImportPdfAttachment(this DocumentUniqueIdentifier Identifier, DiscoDataContext Database, string PdfFilename)
        {
            return ImportPdfAttachment(Identifier, Database, PdfFilename, null);
        }

        public static bool ImportPdfAttachment(this DocumentUniqueIdentifier Identifier, DiscoDataContext Database, string PdfFilename, Image Thumbnail)
        {
            using (var pdfStream = File.OpenRead(PdfFilename))
            {
                return ImportPdfAttachment(Identifier, Database, pdfStream, Thumbnail);
            }
        }

        public static bool ImportPdfAttachment(this DocumentUniqueIdentifier Identifier, DiscoDataContext Database, Stream PdfContent)
        {
            return ImportPdfAttachment(Identifier, Database, PdfContent, null);
        }

        public static bool ImportPdfAttachment(this DocumentUniqueIdentifier Identifier, DiscoDataContext Database, Stream PdfContent, Image Thumbnail)
        {
            string filename;
            string comments;
            IAttachment attachment;

            if (Identifier.DocumentTemplate == null)
            {
                filename = $"{Identifier.Target.AttachmentReferenceId.Replace('\\', '_')}_{Identifier.TimeStamp:yyyyMMdd-HHmmss}.pdf";
                comments = $"Uploaded: {Identifier.TimeStamp:s}";
            }
            else
            {
                filename = $"{Identifier.DocumentTemplateId}_{Identifier.TimeStamp:yyyyMMdd-HHmmss}.pdf";
                comments = string.Format("Generated: {0:s}", Identifier.TimeStamp);
            }

            User creatorUser = UserService.GetUser(Identifier.CreatorId, Database);
            if (creatorUser == null)
            {
                // No Creator User (or Username invalid)
                creatorUser = UserService.CurrentUser;
            }

            switch (Identifier.AttachmentType)
            {
                case AttachmentTypes.Device:
                    Device d = (Device)Identifier.Target;
                    attachment = d.CreateAttachment(Database, creatorUser, filename, DocumentTemplate.PdfMimeType, comments, PdfContent, Identifier.DocumentTemplate, Thumbnail);
                    break;
                case AttachmentTypes.Job:
                    Job j = (Job)Identifier.Target;
                    attachment = j.CreateAttachment(Database, creatorUser, filename, DocumentTemplate.PdfMimeType, comments, PdfContent, Identifier.DocumentTemplate, Thumbnail);
                    break;
                case AttachmentTypes.User:
                    User u = (User)Identifier.Target;
                    attachment = u.CreateAttachment(Database, creatorUser, filename, DocumentTemplate.PdfMimeType, comments, PdfContent, Identifier.DocumentTemplate, Thumbnail);
                    break;
                default:
                    return false;
            }

            if (Identifier.DocumentTemplate != null && !string.IsNullOrWhiteSpace(Identifier.DocumentTemplate.OnImportAttachmentExpression))
            {
                try
                {
                    var expressionResult = Identifier.DocumentTemplate.EvaluateOnAttachmentImportExpression(attachment, Database, creatorUser, Identifier.TimeStamp);
                    DocumentsLog.LogImportAttachmentExpressionEvaluated(Identifier.DocumentTemplate, Identifier.Target, attachment, expressionResult);
                }
                catch (Exception ex)
                {
                    SystemLog.LogException("Document Importer - OnImportAttachmentExpression", ex);
                }
            }
            return true;
        }
    }
}
