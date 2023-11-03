using Microsoft.Win32;

namespace Disco.Services.Interop
{
    public static class MimeTypes
    {
        public static string ResolveMimeType(string Filename)
        {
            string fileExtension;
            if (Filename.Contains("."))
                fileExtension = Filename.Substring(Filename.LastIndexOf(".") + 1).ToLower();
            else
                fileExtension = Filename.ToLower();

            // Try Known Mime Types
            switch (fileExtension)
            {
                case "pdf":
                    return "application/pdf";
                case "doc":
                    return "application/msword";
                case "docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case "docm":
                    return "application/vnd.ms-word.document.macroEnabled.12";
                case "xml":
                    return "text/xml";
                case "xls":
                    return "application/vnd.ms-excel";
                case "xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case "xlsm":
                    return "application/vnd.ms-excel.sheet.macroEnabled.12";
                case "csv":
                    return "application/vnd.ms-excel";
                case "jpg":
                    return "image/jpeg";
                case "gif":
                    return "image/gif";
                case "png":
                    return "image/png";
                case "bmp":
                    return "image/bmp";
                case "avi":
                    return "video/avi";
                case "mpeg":
                case "mpg":
                    return "video/mpeg";
                case "mp3":
                    return "audio/mpeg";
                case "mp4":
                    return "video/mp4";
                case "wmv":
                    return "video/x-ms-wmv";
                case "mov":
                    return "video/quicktime";
                case "js":
                    return "application/javascript";
            }

            // Check System Registry
            try
            {
                RegistryKey regExtensionKey = Registry.ClassesRoot.OpenSubKey("." + fileExtension);
                if (regExtensionKey != null)
                {
                    string regExtensionContentType = regExtensionKey.GetValue("Content Type") as string;
                    if (regExtensionContentType != null)
                    {
                        return regExtensionContentType;
                    }
                }
            }
            catch
            {
                // Ignore Errors
            }

            // Return Default
            return "unknown/unknown";
        }
    }
}
