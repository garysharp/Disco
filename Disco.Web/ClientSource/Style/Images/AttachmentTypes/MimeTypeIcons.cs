using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Web.ClientSource.Style.Images.AttachmentTypes
{
    public static class MimeTypeIcons
    {

        private static IEnumerable<string> DocumentMimeTypes()
        {
            yield return "application/msword";
            yield return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            yield return "application/vnd.ms-word.document.macroEnabled.12";
        }
        private static IEnumerable<string> SpreadsheetMimeTypes()
        {
            yield return "application/vnd.ms-excel";
            yield return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            yield return "application/vnd.ms-excel.sheet.macroEnabled.12";
        }
        private static IEnumerable<string> ArchiveMimeTypes()
        {
            yield return "application/zip";
            yield return "application/gzip";
            yield return "application/x-tar";
            yield return "application/x-zip-compressed";
            yield return "application/x-7z-compressed";
            yield return "application/x-bzip";
            yield return "application/x-bzip2";
            yield return "application/x-gzip";
        }

        public static string Icon(string MimeType)
        {
            // PDF
            if ("application/pdf".Equals(MimeType, StringComparison.OrdinalIgnoreCase))
                return Links.ClientSource.Style.Images.AttachmentTypes.pdf_png;

            // Document icon
            if (DocumentMimeTypes().Any(t => t.Equals(MimeType, StringComparison.OrdinalIgnoreCase)))
                return Links.ClientSource.Style.Images.AttachmentTypes.document_png;

            // Spreadsheet icon
            if (SpreadsheetMimeTypes().Any(t => t.Equals(MimeType, StringComparison.OrdinalIgnoreCase)))
                return Links.ClientSource.Style.Images.AttachmentTypes.spreadsheet_png;

            // Generic 'image' icon
            if (MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return Links.ClientSource.Style.Images.AttachmentTypes.image_png;

            // Generic 'video' icon
            if (MimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
                return Links.ClientSource.Style.Images.AttachmentTypes.video_png;

            if (MimeType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
                return Links.ClientSource.Style.Images.AttachmentTypes.audio_png;

            // Generic 'text' icon
            if (MimeType.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
                return Links.ClientSource.Style.Images.AttachmentTypes.txt_png;

            // Archive icons
            if (ArchiveMimeTypes().Any(t => t.Equals(MimeType, StringComparison.OrdinalIgnoreCase)))
                return Links.ClientSource.Style.Images.AttachmentTypes.archive_png;

            if ("application/binary".Equals(MimeType, StringComparison.OrdinalIgnoreCase) ||
                "application/octet-stream".Equals(MimeType, StringComparison.OrdinalIgnoreCase))
                return Links.ClientSource.Style.Images.AttachmentTypes.binary_png;

            // All other Attachments
            return Links.ClientSource.Style.Images.AttachmentTypes.unknown_png;
        }

    }
}