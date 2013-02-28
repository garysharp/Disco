using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.API.Models.DocumentTemplate
{
    public class ImporterUndetectedFilesModel
    {
        public string Id { get; set; }
        public string Timestamp { get; set; }
        public string TimestampFuzzy { get; set; }
    }
}