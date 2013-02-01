using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Disco.BI.Extensions;
using iTextSharp.text.pdf;

namespace Disco.BI.AttachmentBI
{
    public static class Utilities
    {

        public static bool GenerateThumbnail(Stream Source, string SourceMimeType, Stream OutStream)
        {
            if (Source != null)
            {
                // GDI+ (jpg, png, gif, bmp)
                if (SourceMimeType.Equals("image/jpeg", StringComparison.InvariantCultureIgnoreCase) || SourceMimeType.Contains("jpg") ||
                    SourceMimeType.Equals("image/png", StringComparison.InvariantCultureIgnoreCase) || SourceMimeType.Contains("png") ||
                    SourceMimeType.Equals("image/gif", StringComparison.InvariantCultureIgnoreCase) || SourceMimeType.Contains("gif") ||
                    SourceMimeType.Equals("image/bmp", StringComparison.InvariantCultureIgnoreCase) || SourceMimeType.Contains("bmp"))
                {
                    try
                    {
                        using (Image sourceImage = Image.FromStream(Source))
                        {
                            using (Image thumbImage = sourceImage.ResizeImage(48, 48))
                            {
                                using (Image mimeTypeIcon = Disco.Properties.Resources.MimeType_img16)
                                    thumbImage.EmbedIconOverlay(mimeTypeIcon);
                                thumbImage.SaveJpg(90, OutStream);
                                return true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore Thumbnail Generation exceptions for images
                        //throw;
                    }
                    
                }

                // PDF
                if (SourceMimeType.Equals("application/pdf", StringComparison.InvariantCultureIgnoreCase) || SourceMimeType.Contains("pdf"))
                {
                    PdfReader pdfReader = new PdfReader(Source);
                    try
                    {
                        using (DisposableImageCollection pdfPageImages = pdfReader.PdfPageImages(1))
                        {
                            if (pdfPageImages.Count() > 0)
                            {
                                // Find Biggest Image on Page
                                Image biggestImage = pdfPageImages.OrderByDescending(i => i.Height * i.Width).First();
                                using (Image thumbImage = biggestImage.ResizeImage(48, 48, Brushes.White))
                                {
                                    using (Image mimeTypeIcon = Disco.Properties.Resources.MimeType_pdf16)
                                        thumbImage.EmbedIconOverlay(mimeTypeIcon);
                                    thumbImage.SaveJpg(90, OutStream);
                                    return true;
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (pdfReader != null)
                            pdfReader.Close();
                    }
                }
            }
            return false;
        }
        public static bool GenerateThumbnail(string SourceFilename, string SourceMimeType, string DestinationFilename)
        {
            using (FileStream sourceStream = new FileStream(SourceFilename, FileMode.Open, FileAccess.Read))
            {
                return GenerateThumbnail(sourceStream, SourceMimeType, DestinationFilename);
            }
        }
        public static bool GenerateThumbnail(Stream Source, string SourceMimeType, string DestinationFilename)
        {
            bool result;
            using (FileStream destinationStream = new FileStream(DestinationFilename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                result = GenerateThumbnail(Source, SourceMimeType, destinationStream);
            }
            if (!result && File.Exists(DestinationFilename))
                File.Delete(DestinationFilename);
            
            return result;
        }

    }
}
