using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Disco.ClientBootstrapper
{
    class BootstrapperLoop
    {

        public Thread LoopThread;
        public delegate void LoopCompleteCallback();
        private LoopCompleteCallback mLoopCompleteCallback;
        private IStatus statusUI;
        private string tempWorkingDirectory;
        private StringBuilder errorMessage;
        private Process clientProcess;

        //#if DEBUG
        //        public const string DiscoServerName = "WS-GSHARP";
        //        public const int DiscoServerPort = 57252;
        //#else
        public const string DiscoServerName = "DISCO";
        public const int DiscoServerPort = 9292;
        //#endif

        public BootstrapperLoop(IStatus StatusUI, LoopCompleteCallback Callback)
        {
            statusUI = StatusUI;
            mLoopCompleteCallback = Callback;
            errorMessage = new StringBuilder();
        }

        public void Start()
        {
            LoopThread = new Thread(new ThreadStart(loopHost));
            LoopThread.Start();
        }

        private void loopHost()
        {
            try
            {
                loop();
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

        private void loop()
        {

#if Debug
            statusUI.UpdateStatus("Waiting for Debugger", "Please wait...", true, -1);
            try
            {
                do
                {
                    System.Threading.Thread.Sleep(10);
                } while (!System.Diagnostics.Debugger.IsAttached);
            }
            catch (Exception ex)
            {
                statusUI.UpdateStatus("Error", ex.Message, true, -1);
                return;
            }
#else
            statusUI.UpdateStatus("System Preparation (Bootstrapper)", "Starting", "Please wait...", true, -1);
#endif

            tempWorkingDirectory = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "Disco\\Temp");
            if (!Directory.Exists(tempWorkingDirectory))
                Directory.CreateDirectory(tempWorkingDirectory);

            // Check for Network Connectivity
            statusUI.UpdateStatus(null, "Detecting Network", "Checking network connectivity, Please wait...", true, -1);
            if (!Interop.NetworkInterop.PingDiscoIct(DiscoServerName))
            {
                statusUI.UpdateStatus(null, "Detecting Network", "No network connectivity detected, Diagnosing...", true, -1);
                statusUI_WriteAdapterInfo();

                if (!Interop.NetworkInterop.PingDiscoIct(DiscoServerName))
                {
                    // Check for Wireless
                    var hasWireless = (Interop.NetworkInterop.NetworkAdapters.Count(na => na.IsWireless) > 0);
                    if (hasWireless)
                    {
                        // True: Do wireless loop
                        statusUI.UpdateStatus(null, "Configuring Wireless Network", "Wireless adapter detected, Configuring...", true, -1);
                        Interop.NetworkInterop.ConfigureWireless();
                        statusUI.UpdateStatus(null, "Waiting for Wireless Network", null, true, 0);
                        for (int i = 0; i < 100; i++)
                        {
                            statusUI_WriteAdapterInfo();
                            statusUI.UpdateStatus(null, null, null, true, i);
                            Program.SleepThread(500, false);
                            if (Interop.NetworkInterop.PingDiscoIct(DiscoServerName))
                                break;
                        }
                        if (!Interop.NetworkInterop.PingDiscoIct(DiscoServerName))
                        {
                            statusUI.UpdateStatus(null, "Wireless Network Failed", "Unable to connect to the wireless network, please connect the network cable...", false);
                            Program.SleepThread(3000, false);
                        }
                    }

                    if (!Interop.NetworkInterop.PingDiscoIct(DiscoServerName))
                    {
                        // Instruct user to connect network cable
                        statusUI.UpdateStatus(null, "Please connect the network cable", null);
                        for (int i = 0; i < 100; i++)
                        {
                            statusUI_WriteAdapterInfo();
                            statusUI.UpdateStatus(null, null, null, true, i);
                            Program.SleepThread(500, false);
                            if (Interop.NetworkInterop.PingDiscoIct(DiscoServerName))
                                break;
                        }
                    }
                }

                if (!Interop.NetworkInterop.PingDiscoIct(DiscoServerName))
                {
                    // Client Failed
                    if (mLoopCompleteCallback != null)
                    {
                        mLoopCompleteCallback.BeginInvoke(null, null);
                    }
                    return;
                }
            }

            // Download Client
            statusUI.UpdateStatus(null, "Downloading", "Retrieving Preparation Client, Please wait...", true, -1);
            string clientSourceLocation = Path.Combine(tempWorkingDirectory, "PreparationClient.zip");
            using (var webClient = new WebClient())
            {
                // Don't use a proxy when downloading the Client
                webClient.Proxy = new WebProxy();

                webClient.DownloadFile($"http://{DiscoServerName}:{DiscoServerPort}/Services/Client/PreparationClient", clientSourceLocation);
            }

            // Unzip Client
            statusUI.UpdateStatus(null, "Extracting", "Retrieving Preparation Client, Please wait...", true, -1);
            string clientLocation = Path.Combine(tempWorkingDirectory, "PreparationClient");
            if (Directory.Exists(clientLocation))
                Directory.Delete(clientLocation, true);

            Directory.CreateDirectory(clientLocation);
            using (var clientSource = Ionic.Zip.ZipFile.Read(clientSourceLocation))
            {
                clientSource.ExtractAll(clientLocation, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
            }

            // Launch Client
            statusUI.UpdateStatus("System Preparation (Client)", "Running", "Launching Preparation Client, Please wait...", true, -1);
            ProcessStartInfo clientProcessStart = new ProcessStartInfo(Path.Combine(clientLocation, "Start.bat"))
            {
                WorkingDirectory = clientLocation,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };
            using (clientProcess = Process.Start(clientProcessStart))
            {
                // Read StdOutput until End
                try
                {
                    clientProcess.OutputDataReceived += new DataReceivedEventHandler(clientProcess_OutputDataReceived);
                    clientProcess.BeginOutputReadLine();
                    clientProcess.WaitForExit();
                }
                catch (Exception) { throw; }
                finally
                {
                    try { clientProcess.CloseMainWindow(); }
                    catch (Exception) { }
                }
            }
            clientProcess = null;

            // Cleanup
            if (Directory.Exists(tempWorkingDirectory))
                Directory.Delete(tempWorkingDirectory, true);
            Interop.CertificateInterop.RemoveTempCerts();

            // Pause if Error
            if (errorMessage.Length > 0)
            {
                Program.SleepThread(10000, true);
            }

            // End Of Loop
            if (mLoopCompleteCallback != null)
            {
                mLoopCompleteCallback.BeginInvoke(null, null);
            }
        }

        void statusUI_WriteAdapterInfo()
        {

            var info = new StringBuilder();
            foreach (var na in Interop.NetworkInterop.NetworkAdapters)
            {
                if (na.IsWireless)
                {
                    info.AppendLine($"{na.NetConnectionID}: {na.WirelessConnectionStatusMeaning(na.WirelessConnectionStatus)}");
                }
                else
                {
                    info.AppendLine($"{na.NetConnectionID}: {na.ConnectionStatusMeaning(na.ConnectionStatus)}");
                }
            }
            statusUI.UpdateStatus(null, null, info.ToString());

        }

        void clientProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                Debug.WriteLine($"OUTPUT: {e.Data}");
                var data = e.Data.Substring(1).Split(new char[] { ',' });
                switch (e.Data[0])
                {
                    case '#':
                        if (data.Length == 4)
                        {
                            statusUI.UpdateStatus(null, data[0].Replace("{comma}", ","), data[1].Replace("{comma}", ",").Replace("{newline}", Environment.NewLine), bool.Parse(data[2]), int.Parse(data[3]));
                        }
                        break;
                    case '!':
                        Program.PostBootstrapperActions = new List<string>(data);
                        break;
                }
            }
        }

        //void clientProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    if (!string.IsNullOrEmpty(e.Data))
        //    {
        //        System.Diagnostics.Debug.WriteLine(string.Format("ERROR: {0}", e.Data));
        //        this.errorMessage.AppendLine(e.Data);
        //        statusUI.UpdateStatus(null, "An Error Occurred", this.errorMessage.ToString(), false);
        //    }
        //}

    }
}
