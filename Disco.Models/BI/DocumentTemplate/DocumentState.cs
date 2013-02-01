using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Disco.Models.BI.DocumentTemplates
{
    public class DocumentState : IDisposable
    {
        public int SequenceNumber { get; set; }
        public Hashtable FieldCache { get; set; }
        public Hashtable ScopeCache { get; set; }
        public Hashtable DocumentCache { get; set; }

        public DocumentState(int SequenceNumber)
        {
            this.SequenceNumber = SequenceNumber;
            this.FieldCache = new Hashtable();
            this.ScopeCache = new Hashtable();
            this.DocumentCache = new Hashtable();
        }

        public void FlushFieldCache()
        {
            FlushDictionary(this.FieldCache);
        }
        public void FlushScopeCache()
        {
            FlushFieldCache();
            FlushDictionary(this.ScopeCache);
        }
        public void FlushDocumentCache()
        {
            FlushScopeCache();
            FlushDictionary(this.DocumentCache);
        }
        private static void FlushDictionary(Hashtable d)
        {
            foreach (var key in d.Keys)
            {
                var disposeItem = d[key] as IDisposable;
                if (disposeItem != null)
                    disposeItem.Dispose();
            }
            d.Clear();
        }

        public static DocumentState DefaultState()
        {
            return new DocumentState(1);
        }

        public void Dispose()
        {
            FlushDocumentCache();
        }
    }
}
