using Disco.Data.Repository;
using Disco.Models.Services.Documents;
using System.Collections.Generic;

namespace Disco.Data.Configuration.Modules
{
    public class DocumentsConfiguration : ConfigurationBase
    {
        public DocumentsConfiguration(DiscoDataContext Database) : base(Database) { }

        public override string Scope { get; } = "Documents";

        public List<DocumentTemplatePackage> Packages
        {
            get { return Get<List<DocumentTemplatePackage>>(null); }
            set { Set(value); }
        }

        public DocumentExportOptions LastExportOptions
        {
            get => Get(DocumentExportOptions.DefaultOptions());
            set => Set(value);
        }
    }
}
