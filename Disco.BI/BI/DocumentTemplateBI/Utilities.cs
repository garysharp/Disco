using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Disco.BI.DocumentTemplateBI
{
    public static class Utilities
    {
        public static System.IO.Stream JoinPdfs(params System.IO.Stream[] Pdfs)
        {
            if (Pdfs.Length == 0)
                throw new System.ArgumentNullException("Pdfs");

            // Only One PDF - Possible Reference Bug v's Memory/Speed (Returning Param Memory Stream)
            if (Pdfs.Length == 1)
                return Pdfs[0];

            // Join Pdfs
            System.IO.MemoryStream msBuilder = new System.IO.MemoryStream();
            
            Document pdfDoc = new Document();
            PdfCopy pdfCopy = new PdfCopy(pdfDoc, msBuilder);
            pdfDoc.Open();
            pdfCopy.CloseStream = false;

            for (int i = 0; i < Pdfs.Length; i++)
            {
                System.IO.Stream pdf = Pdfs[i];
                PdfReader pdfReader = new PdfReader(pdf);

                for (int indexPage = 1; indexPage <= pdfReader.NumberOfPages; indexPage++)
                {
                    iTextSharp.text.Rectangle pageSize = pdfReader.GetPageSizeWithRotation(indexPage);
                    PdfImportedPage page = pdfCopy.GetImportedPage(pdfReader, indexPage);
                    pdfDoc.SetPageSize(pageSize);
                    pdfDoc.NewPage();
                    pdfCopy.AddPage(page);
                }

                pdfReader.Close();
            }

            pdfDoc.Close();
            pdfCopy.Close();
            msBuilder.Position = 0;

            return msBuilder;
        }
    }
}
