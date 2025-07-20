using System;
using System.Collections;

namespace Disco.Models.Services.Documents
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
            FieldCache = new Hashtable();
            ScopeCache = new Hashtable();
            DocumentCache = new Hashtable();
        }

        public void FlushFieldCache()
        {
            FlushDictionary(FieldCache);
        }
        public void FlushScopeCache()
        {
            FlushFieldCache();
            FlushDictionary(ScopeCache);
        }
        public void FlushDocumentCache()
        {
            FlushScopeCache();
            FlushDictionary(DocumentCache);
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
