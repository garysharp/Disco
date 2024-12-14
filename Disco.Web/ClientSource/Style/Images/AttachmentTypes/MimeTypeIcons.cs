using System;

namespace Disco.Web.ClientSource.Style.Images.AttachmentTypes
{
    public static class MimeTypeIcons
    {

        public static string Icon(string MimeType)
        {

            switch (MimeType.ToLower())
            {
                case "application/pdf":
                    return Links.ClientSource.Style.Images.AttachmentTypes.pdf_png;
                case "application/msword":
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                case "application/vnd.ms-word.document.macroEnabled.12":
                    return Links.ClientSource.Style.Images.AttachmentTypes.document_png;
            }

            // Generic 'image' icon
            if (MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return Links.ClientSource.Style.Images.AttachmentTypes.image_png;

            // All other Attachments
            return Links.ClientSource.Style.Images.AttachmentTypes.unknown_png;
        }

    }
}