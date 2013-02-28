using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using Disco.Models.Repository;
using Disco.Data.Repository;
using Disco.BI.Extensions;
using System.Web;
using System.Drawing;
using iTextSharp.text.pdf;

namespace Disco.BI.DocumentTemplateBI
{
    class DocumentTemplateQRCodeLocationCache
    {
        private static ConcurrentDictionary<string, List<RectangleF>> _Cache = new ConcurrentDictionary<string, List<RectangleF>>();

        public static List<RectangleF> GetLocations(DocumentTemplate dt, DiscoDataContext dbContext)
        {
            // Check Cache
            List<RectangleF> locations;
            if (_Cache.TryGetValue(dt.Id, out locations))
            {
                return locations;
            }
            // Generate Cache
            return GenerateLocations(dt, dbContext);
        }

        public static bool InvalidateLocations(DocumentTemplate dt)
        {
            List<RectangleF> locations;
            return _Cache.TryRemove(dt.Id, out locations);
        }

        private static bool SetValue(string DocumentTemplateId, List<RectangleF> Locations)
        {
            if (_Cache.ContainsKey(DocumentTemplateId))
            {
                List<RectangleF> oldLocations;
                if (_Cache.TryGetValue(DocumentTemplateId, out oldLocations))
                {
                    return _Cache.TryUpdate(DocumentTemplateId, Locations, oldLocations);
                }
            }
            return _Cache.TryAdd(DocumentTemplateId, Locations);
        }

        internal static List<RectangleF> GenerateLocations(DocumentTemplate dt, DiscoDataContext dbContext)
        {
            string templateFilename = dt.RepositoryFilename(dbContext);
            PdfReader pdfReader = new PdfReader(templateFilename);
            List<RectangleF> locations = new List<RectangleF>();

            if (pdfReader.AcroFields.Fields.ContainsKey("DiscoAttachmentId"))
            {
                foreach (var pdfFieldPosition in pdfReader.AcroFields.GetFieldPositions("DiscoAttachmentId"))
                {
                    var pdfPageSize = pdfReader.GetPageSize(pdfFieldPosition.page);
                    locations.Add(new RectangleF((float)System.Math.Min(1.0, System.Math.Max(0.0, (double)(pdfFieldPosition.position.Left / pdfPageSize.Width) - 0.1)), (float)System.Math.Min(1.0, System.Math.Max(0.0, (double)((pdfPageSize.Height - pdfFieldPosition.position.Top) / pdfPageSize.Height) - 0.1)), (float)System.Math.Min(1.0, System.Math.Max(0.0, (double)(pdfFieldPosition.position.Width / pdfPageSize.Width) + 0.2)), (float)System.Math.Min(1.0, System.Math.Max(0.0, (double)(pdfFieldPosition.position.Height / pdfPageSize.Height) + 0.2))));
                }
            }
            pdfReader.Close();

            // Update Cache
            SetValue(dt.Id, locations);
            return locations;
        }

    }
}
