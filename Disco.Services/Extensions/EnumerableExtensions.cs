using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    public static class OneOf
    {
        public static OneOf<T> Create<T>(T instance)
            => OneOf<T>.Create(instance);
    }
    public struct OneOf<T> : IEnumerable<T>
    {
        private readonly T instance;

        private OneOf(T instance)
        {
            this.instance = instance;
        }

        public static OneOf<T> Create(T instance)
            => new OneOf<T>(instance);

        public IEnumerator<T> GetEnumerator()
            => new OneOfEnumerator(instance);

        IEnumerator IEnumerable.GetEnumerator()
            => new OneOfEnumerator(instance);
        
        private struct OneOfEnumerator : IEnumerator<T>
        {
            private readonly T instance;
            private bool moved;

            public OneOfEnumerator(T instance)
            {
                this.instance = instance;
                moved = false;
            }

            public T Current => instance;

            object IEnumerator.Current => instance;

            public void Dispose() { }

            public bool MoveNext()
            {
                if (!moved)
                {
                    moved = true;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                moved = false;
            }
        }
    }
}
