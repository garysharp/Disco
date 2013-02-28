using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text.pdf;
using System.IO;
using System.Drawing;
using Disco.BI.DocumentTemplateBI.Importer;
using Disco.BI.DocumentTemplateBI;
using System.Drawing.Drawing2D;
using com.google.zxing;
using com.google.zxing.multi.qrcode;
using Disco.Data.Repository;
using System.Web.Caching;
using Disco.BI.Extensions;
using Disco.Models.Repository;
using System.Collections;
using com.google.zxing.common;
using BitMiracle.LibTiff.Classic;

namespace Disco.BI.Interop.Pdf
{
    public static class PdfImporter
    {

        private class DetectImageResult : IDisposable
        {
            public Result Result { get; set; }
            public Point ResultOffset { get; set; }
            public double ResultScale { get; set; }

            public void Dispose()
            {
                // Do Nothing; yet...
            }
        }

        private class DetectPageResult : IDisposable
        {
            public int PageNumber { get; set; }
            public DocumentUniqueIdentifier DetectedIdentifier { get; set; }
            public Disco.BI.Extensions.UtilityExtensions.ImageMontage ThumbnailImage { get; set; }
            public MemoryStream AttachmentThumbnailImage { get; set; }
            public Disco.BI.Extensions.UtilityExtensions.ImageMontage UndetectedPageImage { get; set; }

            public void DrawThumbnailImageResult(DetectImageResult Result, Image DetectedImage)
            {
                if (Result.Result.ResultPoints.Length == 4)
                { // Draw Square on Thumbnail
                    using (Graphics thumbnailGraphics = Graphics.FromImage(ThumbnailImage.Montage))
                    {
                        var thumbnailOffset = ThumbnailImage.MontageSourceImageOffsets[DetectedImage];

                        var linePoints = Result.Result.ResultPoints.Select(p => new Point((int)(thumbnailOffset + ((Result.ResultOffset.X + p.X) * Result.ResultScale * ThumbnailImage.MontageScale)), (int)((p.Y + Result.ResultOffset.Y) * Result.ResultScale * ThumbnailImage.MontageScale))).ToArray();
                        using (GraphicsPath graphicsPath = new GraphicsPath())
                        {
                            for (int linePointIndex = 0; linePointIndex < (linePoints.Length - 1); linePointIndex++)
                                graphicsPath.AddLine(linePoints[linePointIndex], linePoints[linePointIndex + 1]);
                            graphicsPath.AddLine(linePoints[linePoints.Length - 1], linePoints[0]);
                            using (SolidBrush graphicsBrush = new SolidBrush(Color.FromArgb(128, 255, 0, 0)))
                                thumbnailGraphics.FillPath(graphicsBrush, graphicsPath);
                            using (Pen graphicsPen = new Pen(Color.FromArgb(200, 255, 0, 0), 2))
                                thumbnailGraphics.DrawPath(graphicsPen, graphicsPath);
                        }
                    }
                }
            }

            public void Dispose()
            {
                if (ThumbnailImage != null)
                {
                    ThumbnailImage.Dispose();
                    ThumbnailImage = null;
                }
                if (AttachmentThumbnailImage != null)
                {
                    AttachmentThumbnailImage.Dispose();
                    AttachmentThumbnailImage = null;
                }
                if (UndetectedPageImage != null)
                {
                    UndetectedPageImage.Dispose();
                    UndetectedPageImage = null;
                }
            }
        }

        private static DetectImageResult DetectImage(DiscoDataContext dbContext, Bitmap pageImageOriginal, string SessionId, IEnumerable<DocumentTemplate> detectDocumentTemplates)
        {
            Bitmap pageImage = pageImageOriginal;
            double pageImageModifiedScale = 1;

            try
            {
                // Resize if Resolution > 80; Set to 72 Dpi
                if (pageImage.HorizontalResolution > 80 || pageImage.VerticalResolution > 80)
                {
                    pageImageModifiedScale = pageImage.HorizontalResolution / 72;
                    int newWidth = (int)((72 / pageImage.HorizontalResolution) * pageImage.Width);
                    int newHeight = (int)((72 / pageImage.VerticalResolution) * pageImage.Height);
                    pageImage = pageImage.ResizeImage(newWidth, newHeight);
                }

                Result zxingResult = default(Result);
                Point zxingResultOffset = Point.Empty;
                QRCodeMultiReader zxingMfr = new QRCodeMultiReader();
                Hashtable zxingMfrHints = new Hashtable();
                zxingMfrHints.Add(DecodeHintType.TRY_HARDER, true);
                // Look in 'Known' locations
                foreach (DocumentTemplate dt in detectDocumentTemplates)
                {
                    var locationBag = dt.QRCodeLocations(dbContext);
                    foreach (var location in locationBag)
                    {
                        System.Drawing.Rectangle region = new Rectangle(
                            (int)(pageImage.Width * location.Left),
                            (int)(pageImage.Width * location.Top),
                            (int)(pageImage.Width * location.Width),
                            (int)(pageImage.Height * location.Height));
                        RGBLuminanceSource zxingSource;
                        using (Bitmap pageImageRegion = new Bitmap(region.Width, region.Height))
                        {
                            using (Graphics pageImageRegionGraphics = Graphics.FromImage(pageImageRegion))
                            {
                                pageImageRegionGraphics.DrawImage(pageImage, 0, 0, region, GraphicsUnit.Pixel);
                            }
                            zxingSource = new RGBLuminanceSource(pageImageRegion, region.Width, region.Height);
                        }
                        var zxingHB = new HybridBinarizer(zxingSource);
                        var zxingBB = new BinaryBitmap(zxingHB);
                        try
                        {
                            zxingResult = zxingMfr.decode(zxingBB, zxingMfrHints);
                            zxingResultOffset = region.Location;
                            break;
                        }
                        catch (ReaderException)
                        {
                            // Ignore Location Errors
                        }
                    }
                    if (zxingResult != null)
                        break;
                }
                if (zxingResult == null)
                {
                    // Not found with 'known' locations
                    // Try whole image
                    var zxingSource = new RGBLuminanceSource(pageImage, pageImage.Width, pageImage.Height);
                    var zxingHB = new HybridBinarizer(zxingSource);
                    var zxingBB = new BinaryBitmap(zxingHB);
                    try
                    {
                        zxingResult = zxingMfr.decode(zxingBB, zxingMfrHints);
                    }
                    catch (ReaderException)
                    {
                        // Ignore Errors
                    }
                }

                if (zxingResult != null)
                    return new DetectImageResult() { Result = zxingResult, ResultOffset = zxingResultOffset, ResultScale = pageImageModifiedScale };
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (pageImageOriginal != pageImage)
                    pageImage.Dispose();
            }
        }

        private static DetectPageResult DetectPage(DiscoDataContext dbContext, PdfReader pdfReader, int PageNumber, string SessionId, string DataStoreSessionCacheLocation, IEnumerable<DocumentTemplate> detectDocumentTemplates)
        {
            DetectPageResult result = new DetectPageResult() { PageNumber = PageNumber };

            DocumentImporterLog.LogImportPageProgress(SessionId, PageNumber, 10, "Loading Page Images");

            using (DisposableImageCollection pageImages = pdfReader.PdfPageImages(PageNumber))
            {
                if (pageImages.Count > 0)
                {
                    result.ThumbnailImage = pageImages.BuildImageMontage(256, 256);
                    var pageThumbnailFilename = Path.Combine(DataStoreSessionCacheLocation, string.Format("{0}-{1}", SessionId, PageNumber));

                    result.ThumbnailImage.Montage.SavePng(pageThumbnailFilename);
                    DocumentImporterLog.LogImportPageImageUpdate(SessionId, PageNumber);

                    double pageProgressInterval = 90 / pageImages.Count;

                    foreach (var pageImageOriginal in pageImages)
                    {
                        DocumentImporterLog.LogImportPageProgress(SessionId, PageNumber, (int)(10 + (pageProgressInterval * pageImages.IndexOf(pageImageOriginal))), String.Format("Processing Page Image {0} of {1}", pageImages.IndexOf(pageImageOriginal) + 1, pageImages.Count));

                        using (var zxingResult = DetectImage(dbContext, pageImageOriginal, SessionId, detectDocumentTemplates))
                        {
                            if (zxingResult != null)
                            {
                                if (DocumentUniqueIdentifier.IsDocumentUniqueIdentifier(zxingResult.Result.Text))
                                {
                                    result.DrawThumbnailImageResult(zxingResult, pageImageOriginal);
                                    result.ThumbnailImage.Montage.SavePng(pageThumbnailFilename);
                                    DocumentImporterLog.LogImportPageImageUpdate(SessionId, PageNumber);

                                    result.AttachmentThumbnailImage = new MemoryStream();
                                    using (var attachmentThumbImage = pageImages.BuildImageMontage(48, 48, true))
                                    {
                                        using (Image mimeTypeIcon = Disco.Properties.Resources.MimeType_pdf16)
                                            attachmentThumbImage.Montage.EmbedIconOverlay(mimeTypeIcon);
                                        attachmentThumbImage.Montage.SaveJpg(95, result.AttachmentThumbnailImage);
                                    }

                                    result.DetectedIdentifier = new DocumentUniqueIdentifier(zxingResult.Result.Text, PageNumber.ToString());

                                    return result;
                                }
                            }
                        }
                    }

                    // Page Unassigned
                    result.UndetectedPageImage = pageImages.BuildImageMontage(700, 700);
                }

                return result;
            }
        }

        public static bool ProcessPdfAttachment(string Filename, DiscoDataContext dbContext, string SessionId, Cache HttpCache)
        {
            var dataStoreUnassignedLocation = DataStore.CreateLocation(dbContext, "DocumentDropBox_Unassigned");

            DocumentImporterLog.LogImportProgress(SessionId, 0, "Reading File");

            using (FileStream fs = new FileStream(Filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var pdfReader = new PdfReader(fs);

                var pdfPagesAssigned = new Dictionary<int, Tuple<DocumentUniqueIdentifier, byte[]>>();

                var dataStoreSessionPagesCacheLocation = DataStore.CreateLocation(dbContext, "Cache\\DocumentDropBox_SessionPages");
                var detectDocumentTemplates = dbContext.DocumentTemplates.ToArray();

                double progressInterval = 70 / pdfReader.NumberOfPages;

                for (int PageNumber = 1; PageNumber <= pdfReader.NumberOfPages; PageNumber++)
                {
                    DocumentImporterLog.LogImportProgress(SessionId, (int)(PageNumber * progressInterval), string.Format("Processing Page {0} of {1}", PageNumber, pdfReader.NumberOfPages));
                    DocumentImporterLog.LogImportPageStarting(SessionId, PageNumber);

                    using (var pageResult = DetectPage(dbContext, pdfReader, PageNumber, SessionId, dataStoreSessionPagesCacheLocation, detectDocumentTemplates))
                    {
                        if (pageResult.DetectedIdentifier != null)
                        {
                            var docId = pageResult.DetectedIdentifier;
                            pdfPagesAssigned.Add(PageNumber, new Tuple<DocumentUniqueIdentifier, byte[]>(docId, pageResult.AttachmentThumbnailImage.ToArray()));

                            docId.LoadComponents(dbContext);
                            DocumentImporterLog.LogImportPageDetected(SessionId, PageNumber, docId.DocumentUniqueId, docId.DocumentTemplate.Description, docId.DocumentTemplate.Scope, docId.DataId, docId.DataDescription);
                        }
                        else
                        {
                            // Undetected Page - Write Preview-Images while still in Memory
                            DocumentImporterLog.LogImportPageUndetected(SessionId, PageNumber);

                            // Thumbnail:
                            string unassignedImageThumbnailFilename = Path.Combine(dataStoreUnassignedLocation, string.Format("{0}_{1}_thumbnail.png", SessionId, PageNumber));
                            pageResult.ThumbnailImage.Montage.SavePng(unassignedImageThumbnailFilename);
                            // Large Preview
                            string unassignedImageFilename = Path.Combine(dataStoreUnassignedLocation, string.Format("{0}_{1}.jpg", SessionId, PageNumber));
                            pageResult.UndetectedPageImage.Montage.SaveJpg(90, unassignedImageFilename);
                        }
                    }

                }

                // Write out Assigned Documents
                var assignedDocuments = pdfPagesAssigned.GroupBy(u => u.Value.Item1.DocumentUniqueId).ToList();
                if (assignedDocuments.Count > 0)
                {
                    progressInterval = 20 / assignedDocuments.Count;

                    foreach (var documentPortion in assignedDocuments)
                    {
                        DocumentImporterLog.LogImportProgress(SessionId, (int)(70 + (assignedDocuments.IndexOf(documentPortion) * progressInterval)), string.Format("Importing Documents {0} of {1}", assignedDocuments.IndexOf(documentPortion) + 1, assignedDocuments.Count));

                        var documentPortionInfo = documentPortion.First().Value;
                        var documentPortionIdentifier = documentPortionInfo.Item1;
                        var documentPortionThumbnail = documentPortionInfo.Item2;

                        if (!documentPortionIdentifier.LoadComponents(dbContext))
                        {
                            // Unknown Document Unique Id
                            foreach (var dp in documentPortion)
                            {
                                var tag = int.Parse(dp.Value.Item1.Tag);
                                if (pdfPagesAssigned.ContainsKey(tag))
                                    pdfPagesAssigned.Remove(tag);
                            }
                        }
                        else
                        {
                            using (MemoryStream msBuilder = new MemoryStream())
                            {
                                var pdfDoc = new iTextSharp.text.Document();
                                var pdfCopy = new PdfCopy(pdfDoc, msBuilder);

                                pdfDoc.Open();
                                pdfCopy.CloseStream = false;

                                foreach (var dp in documentPortion.OrderBy(dg => dg.Value.Item1.Page))
                                {
                                    var pageSize = pdfReader.GetPageSizeWithRotation(dp.Key);
                                    var page = pdfCopy.GetImportedPage(pdfReader, dp.Key);

                                    pdfDoc.SetPageSize(pageSize);
                                    pdfDoc.NewPage();

                                    pdfCopy.AddPage(page);
                                }

                                pdfDoc.Close();
                                pdfCopy.Close();

                                msBuilder.Position = 0;

                                var attachmentSuccess = documentPortionIdentifier.ImportPdfAttachment(dbContext, msBuilder, documentPortionThumbnail);

                                if (!attachmentSuccess)
                                { // Unable to add Attachment
                                    foreach (var dp in documentPortion)
                                    {
                                        var tag = int.Parse(dp.Value.Item1.Tag);
                                        if (pdfPagesAssigned.ContainsKey(tag))
                                            pdfPagesAssigned.Remove(tag);
                                    }
                                }
                            }
                        }


                    }
                }

                // Write out Unassigned Pages
                List<int> pdfPagesUnassigned = new List<int>();
                for (int PageNumber = 1; PageNumber <= pdfReader.NumberOfPages; PageNumber++)
                    if (!pdfPagesAssigned.ContainsKey(PageNumber))
                        pdfPagesUnassigned.Add(PageNumber);
                if (pdfPagesUnassigned.Count > 0)
                {
                    progressInterval = 10 / pdfPagesUnassigned.Count;
                    //dataStoreUnassignedLocation
                    foreach (var PageNumber in pdfPagesUnassigned)
                    {
                        DocumentImporterLog.LogImportProgress(SessionId, (int)(90 + (pdfPagesUnassigned.IndexOf(PageNumber) * progressInterval)), string.Format("Processing Undetected Documents {0} of {1}", pdfPagesUnassigned.IndexOf(PageNumber) + 1, pdfPagesUnassigned.Count));

                        using (MemoryStream msBuilder = new MemoryStream())
                        {
                            var pdfDoc = new iTextSharp.text.Document();
                            var pdfCopy = new PdfCopy(pdfDoc, msBuilder);

                            pdfDoc.Open();
                            pdfCopy.CloseStream = false;

                            var pageSize = pdfReader.GetPageSizeWithRotation(PageNumber);
                            var page = pdfCopy.GetImportedPage(pdfReader, PageNumber);
                            pdfDoc.SetPageSize(pageSize);
                            pdfDoc.NewPage();

                            pdfCopy.AddPage(page);
                            pdfDoc.Close();
                            pdfCopy.Close();

                            File.WriteAllBytes(Path.Combine(dataStoreUnassignedLocation, string.Format("{0}_{1}.pdf", SessionId, PageNumber)), msBuilder.ToArray());

                            DocumentImporterLog.LogImportPageUndetectedStored(SessionId, PageNumber);
                        }
                    }
                }

            }

            DocumentImporterLog.LogImportProgress(SessionId, 100, "Finished Importing Document");

            return true;
        }
        public static bool ProcessPdfAttachment(string Filename, DiscoDataContext dbContext, string DocumentTemplateId, string DataId, string UserId, DateTime Timestamp)
        {
            using (FileStream fs = new FileStream(Filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                DocumentUniqueIdentifier identifier = new DocumentUniqueIdentifier(DocumentTemplateId, DataId, UserId, Timestamp);
                identifier.LoadComponents(dbContext);
                return identifier.ImportPdfAttachment(dbContext, fs, null);
            }
        }

        public static DisposableImageCollection GetPageImages(PdfReader pdfReader, int PageNumber)
        {
            var pageImages = new DisposableImageCollection();

            var pdfPage = pdfReader.GetPageN(PageNumber);
            PdfDictionary pdfPageResouces = (PdfDictionary)((PdfDictionary)pdfPage.GetDirectObject(PdfName.RESOURCES)).GetDirectObject(PdfName.XOBJECT);

            foreach (var pdfResKey in pdfPageResouces.Keys)
            {
                var pdfRes = pdfPageResouces.GetDirectObject(pdfResKey);
                if (pdfRes.IsStream())
                {
                    var pdfResStream = (PdfStream)pdfRes;
                    var pdfResSubType = pdfResStream.Get(PdfName.SUBTYPE);
                    if (pdfResSubType != null && pdfResSubType == PdfName.IMAGE)
                    {
                        if (pdfResStream.Get(PdfName.FILTER) == PdfName.CCITTFAXDECODE)
                        { // TIFF
                            // Try Using GDI+ for TIFF...
                            var width = ((PdfNumber)(pdfResStream.Get(PdfName.WIDTH))).IntValue;
                            var height = ((PdfNumber)(pdfResStream.Get(PdfName.HEIGHT))).IntValue;
                            var bpc = ((PdfNumber)(pdfResStream.Get(PdfName.BITSPERCOMPONENT))).IntValue;

                            var compressionMethod = Compression.CCITTFAX3;

                            var decodeParams = pdfResStream.GetAsDict(PdfName.DECODEPARMS);
                            if (decodeParams != null && decodeParams.Contains(PdfName.K) && decodeParams.GetAsNumber(PdfName.K).IntValue < 0)
                                compressionMethod = Compression.CCITTFAX4;

                            using (MemoryStream tiffStream = PdfToTiffStream(PdfReader.GetStreamBytesRaw((PRStream)pdfResStream), width, height, bpc, compressionMethod))
                            {
                                pageImages.Add((Bitmap)Bitmap.FromStream(tiffStream));
                            }
                            continue;
                        }
                        if (pdfResStream.Get(PdfName.FILTER) == PdfName.DCTDECODE)
                        { // JPG
                            using (MemoryStream jpgStream = new MemoryStream(PdfReader.GetStreamBytesRaw((PRStream)pdfResStream)))
                            {
                                pageImages.Add((Bitmap)Bitmap.FromStream(jpgStream, true, true));
                            }
                            continue;
                        }
                    }
                }
            }

            return pageImages;
        }

        private static MemoryStream PdfToTiffStream(byte[] PdfStream, int Width, int Height, int BitsPerComponent, Compression CompressionMethod)
        {
            var ms = new MemoryStream();

            Tiff tif = Tiff.ClientOpen("in-memory", "w", ms, new TiffStream());
            tif.SetField(TiffTag.IMAGEWIDTH, Width);
            tif.SetField(TiffTag.IMAGELENGTH, Height);
            tif.SetField(TiffTag.COMPRESSION, CompressionMethod);
            tif.SetField(TiffTag.BITSPERSAMPLE, BitsPerComponent);
            tif.SetField(TiffTag.SAMPLESPERPIXEL, 1);
            tif.WriteRawStrip(0, PdfStream, PdfStream.Length);
            tif.Flush();

            return ms;
        }

    }
}
