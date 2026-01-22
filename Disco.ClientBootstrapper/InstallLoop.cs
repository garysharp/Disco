using System;
using System.Threading;
using System.Threading.Tasks;

namespace Disco.ClientBootstrapper
{
    internal class InstallLoop
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly string installLocation;
        private readonly string wimImageId;
        private readonly string tempPath;
        private readonly Action completeCallback;
        private readonly Uri forcedServerUrl;

        public InstallLoop(string installLocation, string wimImageId, string tempPath, Action completeCallback, Uri forcedServerUrl)
        {
            this.installLocation = installLocation;
            this.wimImageId = wimImageId;
            this.tempPath = tempPath;
            this.completeCallback = completeCallback;
            this.forcedServerUrl = forcedServerUrl;
        }

        public void Start()
        {
            var cancellationToken = cancellationTokenSource.Token;
            Task.Run(async () =>
            {
                try
                {
                    await Interop.InstallInterop.Install(installLocation, wimImageId, tempPath, forcedServerUrl, cancellationToken);
                    completeCallback?.BeginInvoke(null, null);
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(ThreadAbortException))
                        return;
                    if (ex.GetType() == typeof(ThreadInterruptedException))
                        return;
                    if (ex.GetType() == typeof(OperationCanceledException))
                        return;
                    Program.WriteAppError(ex);
                    throw;
                }
            }, cancellationToken);
        }
    }
}
