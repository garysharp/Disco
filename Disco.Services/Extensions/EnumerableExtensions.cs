using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco
{
    public static class EnumerableExtensions
    {

        public static IEnumerable<List<TSource>> Chunk<TSource>(this IEnumerable<TSource> source, int ChunkSize)
        {
            List<TSource> buffer = new List<TSource>(ChunkSize);

            foreach (var item in source)
            {
                buffer.Add(item);

                if (buffer.Count == ChunkSize)
                {
                    yield return buffer;
                    buffer = new List<TSource>();
                }
            }

            // Return any additional items
            if (buffer.Count > 0)
                yield return buffer;
        }

    }
}
