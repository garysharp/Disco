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
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Disco.BI.Interop.Pdf
{
    public static class PdfImporter
    {
        public static RectangleF CalculateLocationRatio(this Result zxingResult, int ImageWidth, int ImageHeight)
        {
            var orderedPoints = zxingResult.ResultPoints.OrderBy(p => p.X * p.Y).ToArray();
            var topLeftPoint = orderedPoints.First();
            var bottomRightPoint = orderedPoints.Last();

            var x = topLeftPoint.X;
            var y = topLeftPoint.Y;
            var width = bottomRightPoint.X - x;
            var height = bottomRightPoint.Y - y;

            return new RectangleF(
                (float)System.Math.Min(1.0, System.Math.Max(0.0, (double)(x / ImageWidth) - 0.05)),
                (float)System.Math.Min(1.0, System.Math.Max(0.0, (double)(y / ImageHeight) - 0.05)),
                (float)System.Math.Min(1.0, System.Math.Max(0.0, (double)(width / ImageWidth) + 0.1)),
                (float)System.Math.Min(1.0, System.Math.Max(0.0, (double)(height / ImageHeight) + 0.1))
                );
        }

        private class DetectImageResult : IDisposable
        {
            public Result Result { get; set; }
            public Point ResultOffset { get; set; }
            public double ResultScale { get; set; }

            public float CalculateRotation()
            {
                var p1 = this.Result.ResultPoints[0];
                var p2 = this.Result.ResultPoints[1];
                double rotOpposite = p1.X - p2.X;
                double rotAdjacent = p1.Y - p2.Y;
                float rotation = 0;

                if (rotOpposite != 0 || rotAdjacent != 0)
                {
                    rotation = (float)(Math.Atan2(rotOpposite, rotAdjacent) * (180 / Math.PI)); // Degrees
                }

                return rotation;
            }

            public void Dispose()
            {
                // Do Nothing; yet...
            }
        }

        private class DetectStateHints
        {
            public List<Tuple<RectangleF, Rotation>> PriorDetections { get; set; }

            public DetectStateHints()
            {
                this.PriorDetections = new List<Tuple<RectangleF, Rotation>>();
            }
        }

        private enum Rotation
        {
            None = 0,
            Degrees90 = 1,
            DegreesNeg90 = 2,
            Degrees180 = 3
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

        private static Tuple<Result, Rectangle, Rotation> DetectImageFromSegment(Bitmap pageImage, QRCodeMultiReader zxingReader, Hashtable zxingReaderHints, RectangleF LocationPercentage, Rotation Rotation)
        {
            System.Drawing.Rectangle region;

            switch (Rotation)
            {
                case Rotation.None: // Original Position
                    region = new Rectangle(
                        (int)(pageImage.Width * LocationPercentage.Left),
                        (int)(pageImage.Height * LocationPercentage.Top),
                        (int)(pageImage.Width * LocationPercentage.Width),
                        (int)(pageImage.Height * LocationPercentage.Height));
                    break;
                case Rotation.Degrees90: // Clockwise 90 degrees
                    region = new Rectangle(
                        (int)(pageImage.Width - (pageImage.Width * (LocationPercentage.Top + LocationPercentage.Height))),
                        (int)(pageImage.Height * LocationPercentage.Left),
                        (int)(pageImage.Width * LocationPercentage.Height),
                        (int)(pageImage.Height * LocationPercentage.Width));
                    break;
                case Rotation.DegreesNeg90: // Anti-clockwise 90 degrees
                    region = new Rectangle(
                        (int)(pageImage.Width * LocationPercentage.Top),
                        (int)(pageImage.Height - (pageImage.Height * (LocationPercentage.Left + LocationPercentage.Width))),
                        (int)(pageImage.Width * LocationPercentage.Height),
                        (int)(pageImage.Height * LocationPercentage.Width));
                    break;
                case Rotation.Degrees180: // 180 degrees
                    region = new Rectangle(
                        (int)(pageImage.Width - (pageImage.Width * (LocationPercentage.Left + LocationPercentage.Width))),
                        (int)(pageImage.Height - (pageImage.Height * (LocationPercentage.Top + LocationPercentage.Height))),
                         (int)(pageImage.Width * LocationPercentage.Width),
                        (int)(pageImage.Height * LocationPercentage.Height));
                    break;
                default:
                    throw new InvalidOperationException("Unknown Rotation");
            }

            LuminanceSource zxingSource;
            using (Bitmap pageImageRegion = new Bitmap(region.Width, region.Height))
            {
                pageImageRegion.SetResolution(pageImage.HorizontalResolution, pageImage.VerticalResolution);

                using (Graphics pageImageRegionGraphics = Graphics.FromImage(pageImageRegion))
                {
                    pageImageRegionGraphics.DrawImage(pageImage, 0, 0, region, GraphicsUnit.Pixel);
                }

                zxingSource = new BitmapLuminanceSource(pageImageRegion);
            }
            var zxingHB = new HybridBinarizer(zxingSource);
            var zxingBB = new BinaryBitmap(zxingHB);
            try
            {
                var zxingResult = zxingReader.decode(zxingBB, zxingReaderHints);
                if (zxingResult != null)
                    return new Tuple<Result, Rectangle, Rotation>(zxingResult, region, Rotation);
            }
            catch (ReaderException)
            {
                // Ignore Location Errors
            }
            return null;
        }

        private static DetectImageResult DetectImage(DiscoDataContext dbContext, Bitmap pageImageOriginal, string SessionId, IEnumerable<DocumentTemplate> detectDocumentTemplates, DetectStateHints StateHints)
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

                Tuple<Result, Rectangle, Rotation> result = default(Tuple<Result, Rectangle, Rotation>);
                QRCodeMultiReader zxingMfr = new QRCodeMultiReader();
                Hashtable zxingMfrHints = new Hashtable();
                zxingMfrHints.Add(DecodeHintType.TRY_HARDER, true);


                // Look in previously found locations
                if (StateHints.PriorDetections.Count > 0)
                {
                    foreach (var previousLocation in StateHints.PriorDetections)
                    {
                        result = DetectImageFromSegment(pageImage, zxingMfr, zxingMfrHints,
                                    previousLocation.Item1, previousLocation.Item2);
                        if (result != null)
                            break;
                    }
                }
                if (result == null)
                {
                    // Try the whole image
                    var zxingSource = new BitmapLuminanceSource(pageImage);
                    var zxingHB = new HybridBinarizer(zxingSource);
                    var zxingBB = new BinaryBitmap(zxingHB);
                    try
                    {
                        var zxingResult = zxingMfr.decode(zxingBB, zxingMfrHints);
                        if (zxingResult != null)
                        {
                            result = new Tuple<Result, Rectangle, Rotation>(zxingResult, new Rectangle(0, 0, pageImage.Width, pageImage.Height), Rotation.None);

                            StateHints.PriorDetections.Insert(0, new Tuple<RectangleF, Rotation>(
                                result.Item1.CalculateLocationRatio(pageImage.Width, pageImage.Height)
                                , Rotation.None));

                        }
                    }
                    catch (ReaderException)
                    {
                        // Ignore Errors
                    }
                }
                if (result == null)
                {
                    // Look in 'Known' locations
                    for (int rotationIndex = 0; rotationIndex < 4; rotationIndex++)
                    {
                        foreach (DocumentTemplate dt in detectDocumentTemplates)
                        {
                            var locationBag = dt.QRCodeLocations(dbContext);
                            foreach (var location in locationBag)
                            {
                                result = DetectImageFromSegment(pageImage, zxingMfr, zxingMfrHints,
                                    location, (Rotation)rotationIndex);

                                StateHints.PriorDetections.Insert(0, new Tuple<RectangleF, Rotation>(location, (Rotation)rotationIndex));
                            }
                            if (result != null)
                                break;
                        }
                        if (result != null)
                            break;
                    }
                }

                if (result != null)

                    return new DetectImageResult() { Result = result.Item1, ResultOffset = result.Item2.Location, ResultScale = pageImageModifiedScale };
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

        private static DetectPageResult DetectPage(DiscoDataContext dbContext, PdfReader pdfReader, int PageNumber, string SessionId, string DataStoreSessionCacheLocation, IEnumerable<DocumentTemplate> detectDocumentTemplates, DetectStateHints StateHints)
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

                        using (var zxingResult = DetectImage(dbContext, pageImageOriginal, SessionId, detectDocumentTemplates, StateHints))
                        {
                            if (zxingResult != null)
                            {
                                if (DocumentUniqueIdentifier.IsDocumentUniqueIdentifier(zxingResult.Result.Text))
                                {
                                    float imageRotation = zxingResult.CalculateRotation();

                                    result.DrawThumbnailImageResult(zxingResult, pageImageOriginal);

                                    if (imageRotation != 0)
                                    {
                                        var preImageRotation = result.ThumbnailImage.Montage;
                                        result.ThumbnailImage.Montage = result.ThumbnailImage.Montage.RotateImage(imageRotation);
                                        preImageRotation.Dispose();
                                    }

                                    result.ThumbnailImage.Montage.SavePng(pageThumbnailFilename);
                                    DocumentImporterLog.LogImportPageImageUpdate(SessionId, PageNumber);

                                    result.AttachmentThumbnailImage = new MemoryStream();
                                    using (var attachmentThumbImage = pageImages.BuildImageMontage(48, 48, true))
                                    {
                                        using (Image mimeTypeIcon = Disco.Properties.Resources.MimeType_pdf16)
                                            attachmentThumbImage.Montage.EmbedIconOverlay(mimeTypeIcon);

                                        if (imageRotation != 0)
                                        {
                                            var preImageRotation = attachmentThumbImage.Montage;
                                            attachmentThumbImage.Montage = attachmentThumbImage.Montage.RotateImage(imageRotation, Brushes.White);
                                            preImageRotation.Dispose();
                                        }

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

                DetectStateHints detectStateHints = new DetectStateHints();

                for (int PageNumber = 1; PageNumber <= pdfReader.NumberOfPages; PageNumber++)
                {
                    DocumentImporterLog.LogImportProgress(SessionId, (int)(PageNumber * progressInterval), string.Format("Processing Page {0} of {1}", PageNumber, pdfReader.NumberOfPages));
                    DocumentImporterLog.LogImportPageStarting(SessionId, PageNumber);

                    using (var pageResult = DetectPage(dbContext, pdfReader, PageNumber, SessionId, dataStoreSessionPagesCacheLocation, detectDocumentTemplates, detectStateHints))
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

    /*
* Copyright 2012 ZXing.Net authors
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
    /// <summary>
    /// The base class for luminance sources which supports 
    /// cropping and rotating based upon the luminance values.
    /// </summary>
    public abstract class BaseLuminanceSource : LuminanceSource
    {
        /// <summary>
        /// 
        /// </summary>
        protected sbyte[] luminances;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseLuminanceSource"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        protected BaseLuminanceSource(int width, int height)
            : base(width, height)
        {
            luminances = new sbyte[width * height];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseLuminanceSource"/> class.
        /// </summary>
        /// <param name="luminanceArray">The luminance array.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        protected BaseLuminanceSource(sbyte[] luminanceArray, int width, int height)
            : base(width, height)
        {
            luminances = luminanceArray;
            //Buffer.BlockCopy(luminanceArray, 0, luminances, 0, width * height);
        }

        /// <summary>
        /// Fetches one row of luminance data from the underlying platform's bitmap. Values range from
        /// 0 (black) to 255 (white). It is preferable for implementations of this method
        /// to only fetch this row rather than the whole image, since no 2D Readers may be installed and
        /// getMatrix() may never be called.
        /// </summary>
        /// <param name="y">The row to fetch, 0 &lt;= y &lt; Height.</param>
        /// <param name="row">An optional preallocated array. If null or too small, it will be ignored.
        /// Always use the returned object, and ignore the .length of the array.</param>
        /// <returns>
        /// An array containing the luminance data.
        /// </returns>
        override public sbyte[] getRow(int y, sbyte[] row)
        {
            int width = Width;
            if (row == null || row.Length < width)
            {
                row = new sbyte[width];
            }
            //for (int i = 0; i < width; i++)
            //    row[i] = luminances[y * width + i];
            Buffer.BlockCopy(luminances, y * width, row, 0, width);
            return row;
        }

        public override sbyte[] Matrix
        {
            get { return luminances; }
        }

        /// <summary>
        /// Returns a new object with rotated image data by 90 degrees counterclockwise.
        /// Only callable if {@link #isRotateSupported()} is true.
        /// </summary>
        /// <returns>
        /// A rotated version of this object.
        /// </returns>
        public override LuminanceSource rotateCounterClockwise()
        {
            var rotatedLuminances = new sbyte[Width * Height];
            var newWidth = Height;
            var newHeight = Width;
            var localLuminances = Matrix;
            for (var yold = 0; yold < Height; yold++)
            {
                for (var xold = 0; xold < Width; xold++)
                {
                    var ynew = xold;
                    var xnew = newWidth - yold - 1;
                    rotatedLuminances[ynew * newWidth + xnew] = localLuminances[yold * Width + xold];
                }
            }
            return CreateLuminanceSource(rotatedLuminances, newWidth, newHeight);
        }

        /// <summary>
        /// </summary>
        /// <returns> Whether this subclass supports counter-clockwise rotation.</returns>
        public override bool RotateSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns a new object with cropped image data. Implementations may keep a reference to the
        /// original data rather than a copy. Only callable if CropSupported is true.
        /// </summary>
        /// <param name="left">The left coordinate, 0 &lt;= left &lt; Width.</param>
        /// <param name="top">The top coordinate, 0 &lt;= top &lt;= Height.</param>
        /// <param name="width">The width of the rectangle to crop.</param>
        /// <param name="height">The height of the rectangle to crop.</param>
        /// <returns>
        /// A cropped version of this object.
        /// </returns>
        public override LuminanceSource crop(int left, int top, int width, int height)
        {
            if (left + width > Width || top + height > Height)
            {
                throw new ArgumentException("Crop rectangle does not fit within image data.");
            }
            var croppedLuminances = new sbyte[width * height];
            for (int yold = top, ynew = 0; yold < height; yold++, ynew++)
            {
                for (int xold = left, xnew = 0; xold < width; xold++, xnew++)
                {
                    croppedLuminances[ynew * width + xnew] = luminances[yold * Width + xold];
                }
            }
            return CreateLuminanceSource(croppedLuminances, width, height);
        }

        /// <summary>
        /// </summary>
        /// <returns> Whether this subclass supports cropping.</returns>
        public override bool CropSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Should create a new luminance source with the right class type.
        /// The method is used in methods crop and rotate.
        /// </summary>
        /// <param name="newLuminances">The new luminances.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        protected abstract LuminanceSource CreateLuminanceSource(sbyte[] newLuminances, int width, int height);
    }
    public partial class BitmapLuminanceSource : BaseLuminanceSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapLuminanceSource"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        protected BitmapLuminanceSource(int width, int height)
            : base(width, height)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapLuminanceSource"/> class
        /// with the image of a Bitmap instance
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        public BitmapLuminanceSource(Bitmap bitmap)
            : base(bitmap.Width, bitmap.Height)
        {
            var height = bitmap.Height;
            var width = bitmap.Width;

            // In order to measure pure decoding speed, we convert the entire image to a greyscale array
            luminances = new sbyte[width * height];

            // The underlying raster of image consists of bytes with the luminance values
#if WindowsCE
         var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
#else
            var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
#endif
            try
            {
                var stride = Math.Abs(data.Stride);
                var pixelWidth = stride / width;

                if (pixelWidth > 4)
                {
                    // old slow way for unsupported bit depth
                    Color c;
                    for (int y = 0; y < height; y++)
                    {
                        int offset = y * width;
                        for (int x = 0; x < width; x++)
                        {
                            c = bitmap.GetPixel(x, y);
                            luminances[offset + x] = (sbyte)(0.3 * c.R + 0.59 * c.G + 0.11 * c.B + 0.01);
                        }
                    }
                }
                else
                {
                    var strideStep = data.Stride;
                    var buffer = new byte[stride];
                    var ptrInBitmap = data.Scan0;

#if !WindowsCE
                    // prepare palette for 1 and 8 bit indexed bitmaps
                    var luminancePalette = new sbyte[bitmap.Palette.Entries.Length];
                    for (var index = 0; index < bitmap.Palette.Entries.Length; index++)
                    {
                        var color = bitmap.Palette.Entries[index];
                        luminancePalette[index] = (sbyte)(0.3 * color.R +
                                                          0.59 * color.G +
                                                          0.11 * color.B + 0.01);
                    }
                    if (bitmap.PixelFormat == PixelFormat.Format32bppArgb ||
                        bitmap.PixelFormat == PixelFormat.Format32bppPArgb)
                    {
                        pixelWidth = 40;
                    }
#endif

                    for (int y = 0; y < height; y++)
                    {
                        // copy a scanline not the whole bitmap because of memory usage
                        Marshal.Copy(ptrInBitmap, buffer, 0, stride);
#if NET40
                  ptrInBitmap = IntPtr.Add(ptrInBitmap, strideStep);
#else
                        ptrInBitmap = new IntPtr(ptrInBitmap.ToInt64() + strideStep);
#endif
                        var offset = y * width;
                        switch (pixelWidth)
                        {
#if !WindowsCE
                            case 0:
                                for (int x = 0; x * 8 < width; x++)
                                {
                                    for (int subX = 0; subX < 8 && 8 * x + subX < width; subX++)
                                    {
                                        var index = (buffer[x] >> (7 - subX)) & 1;
                                        luminances[offset + 8 * x + subX] = luminancePalette[index];
                                    }
                                }
                                break;
                            case 1:
                                for (int x = 0; x < width; x++)
                                {
                                    luminances[offset + x] = luminancePalette[buffer[x]];
                                }
                                break;
#endif
                            case 2:
                                // should be RGB565 or RGB555, assume RGB565
                                {
                                    for (int index = 0, x = 0; index < 2 * width; index += 2, x++)
                                    {
                                        var byte1 = buffer[index];
                                        var byte2 = buffer[index + 1];

                                        var b5 = byte1 & 0x1F;
                                        var g5 = (((byte1 & 0xE0) >> 5) | ((byte2 & 0x03) << 3)) & 0x1F;
                                        var r5 = (byte2 >> 2) & 0x1F;
                                        var r8 = (r5 * 527 + 23) >> 6;
                                        var g8 = (g5 * 527 + 23) >> 6;
                                        var b8 = (b5 * 527 + 23) >> 6;

                                        luminances[offset + x] = (sbyte)(0.3 * r8 + 0.59 * g8 + 0.11 * b8 + 0.01);
                                    }
                                }
                                break;
                            case 3:
                                for (int x = 0; x < width; x++)
                                {
                                    var luminance = (sbyte)(0.3 * buffer[x * 3] +
                                                           0.59 * buffer[x * 3 + 1] +
                                                           0.11 * buffer[x * 3 + 2] + 0.01);
                                    luminances[offset + x] = luminance;
                                }
                                break;
                            case 4:
                                // 4 bytes without alpha channel value
                                for (int x = 0; x < width; x++)
                                {
                                    var luminance = (sbyte)(0.30 * buffer[x * 4] +
                                                           0.59 * buffer[x * 4 + 1] +
                                                           0.11 * buffer[x * 4 + 2] + 0.01);

                                    luminances[offset + x] = luminance;
                                }
                                break;
                            case 40:
                                // with alpha channel; some barcodes are completely black if you
                                // only look at the r, g and b channel but the alpha channel controls
                                // the view
                                for (int x = 0; x < width; x++)
                                {
                                    var luminance = (sbyte)(0.30 * buffer[x * 4] +
                                                           0.59 * buffer[x * 4 + 1] +
                                                           0.11 * buffer[x * 4 + 2] + 0.01);

                                    // calculating the resulting luminance based upon a white background
                                    // var alpha = buffer[x * pixelWidth + 3] / 255.0;
                                    // luminance = (byte)(luminance * alpha + 255 * (1 - alpha));
                                    var alpha = buffer[x * 4 + 3];
                                    luminance = (sbyte)(((luminance * alpha) >> 8) + (255 * (255 - alpha) >> 8));
                                    luminances[offset + x] = luminance;
                                }
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        /// <summary>
        /// Should create a new luminance source with the right class type.
        /// The method is used in methods crop and rotate.
        /// </summary>
        /// <param name="newLuminances">The new luminances.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        protected override LuminanceSource CreateLuminanceSource(sbyte[] newLuminances, int width, int height)
        {
            return new BitmapLuminanceSource(width, height) { luminances = newLuminances };
        }
    }

}
