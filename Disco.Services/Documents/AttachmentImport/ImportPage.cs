using Disco.Data.Repository;
using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using ZXing;
using ZXing.Common;
using ZXing.Multi.QrCode;

namespace Disco.Services.Documents.AttachmentImport
{
    internal class ImportPage : IDisposable
    {
        public DiscoDataContext Database { get; private set; }
        public string SessionId { get; private set; }
        public PdfDocument PdfiumDocument { get; private set; }
        public int PageIndex { get; private set; }

        public DocumentUniqueIdentifier Identifier { get; private set; }

        private Result qrCodeResult;
        private float qrCodeResultScale;
        private Image renderedImage;
        private Bitmap renderedThumbnail;
        private RotateFlipType detectedRotation;

        public ImportPage(DiscoDataContext Database, string SessionId, PdfDocument PdfiumDocument, int PageIndex)
        {
            this.Database = Database;
            this.SessionId = SessionId;
            this.PdfiumDocument = PdfiumDocument;
            this.PageIndex = PageIndex;
        }

        public bool IsDetected
        {
            get
            {
                return Identifier != null;
            }
        }

        public Image Image
        {
            get
            {
                return GetRenderedImage();
            }
        }

        public Bitmap Thumbnail
        {
            get
            {
                return GetRenderedThumbnail();
            }
        }

        public bool IsValidAttachment
        {
            get
            {
                return Identifier != null &&
                    Identifier.Creator != null &&
                    Identifier.Target != null &&
                    (Identifier.DocumentTemplate != null || Identifier.AttachmentType.HasValue);
            }
        }

        public int Rotation
        {
            get
            {
                switch (detectedRotation)
                {
                    case RotateFlipType.Rotate90FlipNone:
                        return 90;
                    case RotateFlipType.Rotate180FlipNone:
                        return 180;
                    case RotateFlipType.Rotate270FlipNone:
                        return 270;
                    default:
                        return 0;
                }
            }
        }

        public void WriteThumbnailSessionCache()
        {
            var sessionCacheLocation = DataStore.CreateLocation(Database, "Cache\\DocumentDropBox_SessionPages");
            var filename = Path.Combine(sessionCacheLocation, $"{SessionId}-{PageIndex + 1}");

            Thumbnail.SavePng(filename);
        }

        public void WriteUndetectedImages()
        {
            var undetectedLocation = DataStore.CreateLocation(Database, "DocumentDropBox_Unassigned");
            var filename = Path.Combine(undetectedLocation, $"{SessionId}_{PageIndex + 1}_thumbnail.png");
            Thumbnail.SavePng(filename);

            using (var largePreview = Image.ResizeImage(700, 700))
            {
                filename = Path.Combine(undetectedLocation, $"{SessionId}_{PageIndex + 1}.jpg");
                largePreview.SaveJpg(90, filename);
            }
        }

        public bool DetectQRCode()
        {
            var qrReader = new QRCodeMultiReader();

            var qrImageSource = new BitmapLuminanceSource((Bitmap)Image);

            var qrBinarizer = new HybridBinarizer(qrImageSource);
            var qrBinaryBitmap = new BinaryBitmap(qrBinarizer);

            Result DetectDocumentUniqueIdentifier(BinaryBitmap bitmap)
            {
                var qrReaderHints = new Dictionary<DecodeHintType, object>() {
                    { DecodeHintType.TRY_HARDER, true }
                };

                var qrCodeResults = qrReader.decodeMultiple(bitmap, qrReaderHints);

                if (qrCodeResults != null && qrCodeResults.Length > 0)
                {
                    if (qrCodeResults.Length > 1)
                    {
                        // multiple qr codes on page, test for byte-mark
                        foreach (var qr in qrCodeResults)
                        {
                            if (qr.ResultMetadata.TryGetValue(ResultMetadataType.BYTE_SEGMENTS, out var byteSegments))
                            {
                                var qrBytes = ((List<byte[]>)byteSegments)[0];
                                if (DocumentUniqueIdentifier.IsDocumentUniqueIdentifier(qrBytes))
                                    return qr;
                            }
                        }
                    }
                    return qrCodeResults[0];
                }
                return null;
            }

            try
            {
                qrCodeResult = DetectDocumentUniqueIdentifier(qrBinaryBitmap);
                qrCodeResultScale = 1F;
            }
            catch (ReaderException)
            {
                // QR Detection Failed
                qrCodeResult = null;
            }

            if (qrCodeResult == null)
            {
                var sizePoints = PdfiumDocument.PageSizes[PageIndex];

                // Try at 175%
                using (var image = PdfiumDocument.Render(PageIndex, (int)(sizePoints.Width * 1.75), (int)(sizePoints.Height * 1.75), 72F, 72F, false))
                {
                    qrImageSource = new BitmapLuminanceSource((Bitmap)image);

                    // Try Entire Image
                    qrBinarizer = new HybridBinarizer(qrImageSource);
                    qrBinaryBitmap = new BinaryBitmap(qrBinarizer);

                    try
                    {
                        qrCodeResult = DetectDocumentUniqueIdentifier(qrBinaryBitmap);
                        qrCodeResultScale = 1.75F;
                    }
                    catch (ReaderException)
                    {
                        // QR Detection Failed
                        qrCodeResult = null;
                    }
                }

                if (qrCodeResult == null)
                {
                    // Try at 200%
                    using (var image = PdfiumDocument.Render(PageIndex, (int)(sizePoints.Width * 2), (int)(sizePoints.Height * 2), 72F, 72F, false))
                    {
                        qrImageSource = new BitmapLuminanceSource((Bitmap)image);

                        // Try Entire Image
                        qrBinarizer = new HybridBinarizer(qrImageSource);
                        qrBinaryBitmap = new BinaryBitmap(qrBinarizer);

                        try
                        {
                            qrCodeResult = DetectDocumentUniqueIdentifier(qrBinaryBitmap);
                            qrCodeResultScale = 2F;
                        }
                        catch (ReaderException)
                        {
                            // QR Detection Failed
                            qrCodeResult = null;
                        }
                    }
                }
            }

            if (qrCodeResult != null)
            {
                // Detect Rotation
                var rotationAngle = Math.Atan2(
                    qrCodeResult.ResultPoints[2].Y - qrCodeResult.ResultPoints[1].Y,
                    qrCodeResult.ResultPoints[2].X - qrCodeResult.ResultPoints[1].X) * 180 / Math.PI;
                if (rotationAngle <= 45 || rotationAngle > 315)
                {
                    detectedRotation = RotateFlipType.RotateNoneFlipNone;
                }
                else if (rotationAngle <= 135)
                {
                    detectedRotation = RotateFlipType.Rotate270FlipNone;
                }
                else if (rotationAngle <= 225)
                {
                    detectedRotation = RotateFlipType.Rotate180FlipNone;
                }
                else
                {
                    detectedRotation = RotateFlipType.Rotate90FlipNone;
                }

                // Reset Thumbnail
                if (renderedThumbnail != null)
                {
                    renderedThumbnail.Dispose();
                    renderedThumbnail = null;
                }

                // Try binary encoding (from v2)
                if (qrCodeResult.ResultMetadata.ContainsKey(ResultMetadataType.BYTE_SEGMENTS))
                {
                    var byteSegments = (List<byte[]>)qrCodeResult.ResultMetadata[ResultMetadataType.BYTE_SEGMENTS];
                    var qrBytes = byteSegments[0];
                    if (DocumentUniqueIdentifier.IsDocumentUniqueIdentifier(qrBytes))
                    {
                        Identifier = DocumentUniqueIdentifier.Parse(Database, qrBytes);
                    }
                }
                // Fall back to v1
                if (Identifier == null)
                {
                    Identifier = DocumentUniqueIdentifier.Parse(Database, qrCodeResult.Text);
                }

                return true;
            }

            return false;
        }

        public Bitmap GetAttachmentThumbnail()
        {
            var thumbnail = renderedImage.ResizeImage(48, 48, Brushes.White);

            // Draw Rotation
            if (detectedRotation != RotateFlipType.RotateNoneFlipNone)
            {
                thumbnail.RotateFlip(detectedRotation);
            }

            // Add PDF Icon overlay
            using (Image mimeTypeIcon = Properties.Resources.MimeType_pdf16)
            {
                thumbnail.EmbedIconOverlay(mimeTypeIcon);
            }

            return thumbnail;
        }

        private Image GetRenderedImage()
        {
            if (renderedImage == null)
            {
                var pageSize = PdfiumDocument.PageSizes[PageIndex];

                renderedImage = PdfiumDocument.Render(PageIndex, (int)pageSize.Width, (int)pageSize.Height, 72F, 72F, true);
            }

            return renderedImage;
        }

        private Bitmap GetRenderedThumbnail()
        {
            if (renderedThumbnail == null)
            {
                renderedThumbnail = GetRenderedImage().ResizeImage(256, 256, Brushes.White);

                if (qrCodeResult != null && qrCodeResult.ResultPoints.Length == 4)
                {
                    var thumbnailOffset = renderedImage.CalculateResize(renderedThumbnail.Width, renderedThumbnail.Height, out var thumbnailScale);
                    thumbnailScale = thumbnailScale / qrCodeResultScale;

                    using (Graphics thumbnailGraphics = Graphics.FromImage(renderedThumbnail))
                    {
                        // Draw Square on QR Code
                        var linePoints = qrCodeResult.ResultPoints.Select(p => new Point((int)(thumbnailOffset.X + (p.X * thumbnailScale)), (int)(thumbnailOffset.Y + (p.Y * thumbnailScale)))).ToList();
                        using (GraphicsPath graphicsPath = new GraphicsPath())
                        {
                            for (int linePointIndex = 0; linePointIndex < (linePoints.Count - 1); linePointIndex++)
                                graphicsPath.AddLine(linePoints[linePointIndex], linePoints[linePointIndex + 1]);

                            graphicsPath.AddLine(linePoints[linePoints.Count - 1], linePoints[0]);

                            using (SolidBrush graphicsBrush = new SolidBrush(Color.FromArgb(128, 255, 0, 0)))
                                thumbnailGraphics.FillPath(graphicsBrush, graphicsPath);

                            using (Pen graphicsPen = new Pen(Color.FromArgb(200, 255, 0, 0), 2))
                                thumbnailGraphics.DrawPath(graphicsPen, graphicsPath);
                        }

                        // Draw Rotation
                        if (detectedRotation != RotateFlipType.RotateNoneFlipNone)
                        {
                            renderedThumbnail.RotateFlip(detectedRotation);
                        }
                    }
                }
            }
            return renderedThumbnail;
        }
        public void Dispose()
        {
            if (renderedImage != null)
                renderedImage.Dispose();
            if (renderedThumbnail != null)
                renderedThumbnail.Dispose();
        }
    }
}
