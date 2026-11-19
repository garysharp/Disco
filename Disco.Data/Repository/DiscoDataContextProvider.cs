using System;
using System.Threading;

namespace Disco.Data.Repository
{
    public static class DiscoDataContextProvider
    {
        private static readonly IDiscoDataContextFactory defaultFactory = new DefaultDiscoDataContextFactory();
        private static readonly AsyncLocal<IDiscoDataContextFactory> currentFactory = new AsyncLocal<IDiscoDataContextFactory>();

        public static DiscoDataContext Create()
        {
            return (currentFactory.Value ?? defaultFactory).Create();
        }

        public static IDisposable Push(IDiscoDataContextFactory factory)
        {
            var previousFactory = currentFactory.Value;
            currentFactory.Value = factory
                ?? throw new ArgumentNullException(nameof(factory));

            return new DisposableScope(() => currentFactory.Value = previousFactory);
        }

        private sealed class DefaultDiscoDataContextFactory : IDiscoDataContextFactory
        {
            public DiscoDataContext Create()
            {
                return new DiscoDataContext();
            }
        }

        private sealed class DisposableScope : IDisposable
        {
            private readonly Action onDispose;
            private bool disposed;

            public DisposableScope(Action onDispose)
            {
                this.onDispose = onDispose;
            }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;
                onDispose();
            }
        }
    }
}
