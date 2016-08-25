using Disco.Data.Repository;
using Disco.Models.Repository;
using System.IO;

namespace Disco.Services
{
    public static class DocumentTemplateDataStoreExtensions
    {
        public static string RepositoryFilename(this DocumentTemplate dt, DiscoDataContext Database)
        {
            return Path.Combine(DataStore.CreateLocation(Database, "DocumentTemplates"), string.Format("{0}.pdf", dt.Id));
        }
        public static string SavePdfTemplate(this DocumentTemplate dt, DiscoDataContext Database, Stream TemplateFile)
        {
            string filePath = dt.RepositoryFilename(Database);
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                TemplateFile.CopyTo(fs);
            }
            Expressions.ExpressionCache.InvalidModule(string.Format(DocumentTemplateExpressionExtensions.CacheTemplate, dt.Id));
            return filePath;
        }
    }
}
