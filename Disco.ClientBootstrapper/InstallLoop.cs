using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public InstallLoop(string InstallLocation, string WimImageId = null)
        {
            this.InstallLocation = InstallLocation;
            this.WimImageId = WimImageId;
        }

        public void Start(CompleteCallback Callback)
        {
            this.mCompleteCallback = Callback;
            this.LoopThread = new Thread(new ThreadStart(loopHost));
            this.LoopThread.Start();
        }
        private void loopHost()
        {
            try
            {

                //Program.Status.UpdateStatus(null, null, "Testing UI");
                //Program.SleepThread(5000, false);
                Interop.InstallInterop.Install(this.InstallLocation, this.WimImageId);
                if (this.mCompleteCallback != null)
                {
                    this.mCompleteCallback.BeginInvoke(null, null);
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
