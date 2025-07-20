using System;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Extensions
{
    public class FileContentSpanResult : FileResult
    {
        public byte[] FileBuffer { get; private set; }
        public int Start { get; private set; }
        public int Length { get; private set; }

        public FileContentSpanResult(byte[] fileBuffer, int start, int length, string contentType) : base(contentType)
        {
            if (fileBuffer == null)
                throw new ArgumentNullException(nameof(fileBuffer));

            if (start < 0 || start >= fileBuffer.Length)
                throw new ArgumentOutOfRangeException(nameof(start));

            if (start + length > fileBuffer.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            FileBuffer = fileBuffer;
            Start = start;
            Length = length;
        }

        protected override void WriteFile(HttpResponseBase response)
        {
            response.OutputStream.Write(FileBuffer, Start, Length);
        }
    }
}
