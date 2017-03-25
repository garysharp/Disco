using System.Web.Mvc;

namespace Disco.Web.Extensions
{
    public static class FileExtensions
    {

        public static FileContentSpanResult File(this IController controller, byte[] fileBuffer, int start, int length, string contentType, string fileDownloadName)
        {
            return new FileContentSpanResult(fileBuffer, start, length, contentType)
            {
                FileDownloadName = fileDownloadName
            };
        }

    }
}
