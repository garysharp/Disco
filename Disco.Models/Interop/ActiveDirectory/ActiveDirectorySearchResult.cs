using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Interop.ActiveDirectory
{
    public class ActiveDirectorySearchResult
    {
        public ActiveDirectoryDomain Domain { get; set; }
        public string SearchRoot { get; set; }
        public SearchResult Result { get; set; }
    }
}
