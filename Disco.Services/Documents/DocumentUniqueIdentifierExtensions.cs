using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services
{
    public static class DocumentUniqueIdentifierExtensions
    {

        public static DocumentUniqueIdentifier CreateUniqueIdentifier(this DocumentTemplate Template, DiscoDataContext Database, IAttachmentTarget Target, User Creator, DateTime Timestamp, int PageIndex)
        {
            return DocumentUniqueIdentifier.Create(Database, Template, Target, Creator, Timestamp, PageIndex);
        }

    }
}
