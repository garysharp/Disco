using System;
using System.Threading;

namespace Disco.ClientBootstrapper
{
    class InstallLoop
    {

        public Thread LoopThread;
        public delegate void CompleteCallback();
        private CompleteCallback mCompleteCallback;
        private string InstallLocation;
        private string WimImageId;
        private string TempPath;

        public InstallLoop(string InstallLocation, string WimImageId, string TempPath)
        {
            this.InstallLocation = InstallLocation;
            this.WimImageId = WimImageId;
            this.TempPath = TempPath;
        }

        public void Start(CompleteCallback Callback)
        {
            mCompleteCallback = Callback;
            LoopThread = new Thread(new ThreadStart(loopHost));
            LoopThread.Start();
        }
        private void loopHost()
        {
            try
            {

                //Program.Status.UpdateStatus(null, null, "Testing UI");
                //Program.SleepThread(5000, false);
                Interop.InstallInterop.Install(InstallLocation, WimImageId, TempPath);
                if (mCompleteCallback != null)
                {
                    mCompleteCallback.BeginInvoke(null, null);
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(ThreadAbortException))
                    return;
                if (ex.GetType() == typeof(ThreadInterruptedException))
                    return;
                Program.WriteAppError(ex);
                throw;
            }
        }

    }
}
