using Disco.Client.Interop;
using Disco.ClientBootstrapper.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Disco.ClientBootstrapper
{
    internal class BootstrapperLoop
    {
        private readonly Func<CancellationToken, Task> completeCallback;
        private readonly CancellationToken cancellationToken;
        private readonly IStatus statusUI;
        private readonly Uri forcedServerUrl;
        private string tempWorkingDirectory;
        private Process clientProcess;

        public BootstrapperLoop(IStatus statusUI, Uri forcedServerUrl, Func<CancellationToken, Task> callback, CancellationToken cancellationToken)
        {
            this.statusUI = statusUI;
            this.forcedServerUrl = forcedServerUrl;
            completeCallback = callback;
            this.cancellationToken = cancellationToken;
        }

        public void Start()
        {
            Task.Factory.StartNew(async () =>
            {
                await Loop(forcedServerUrl, cancellationToken);
            }, cancellationToken);
        }

        private async Task Loop(Uri forcedServerUrl, CancellationToken cancellationToken)
        {
            try
            {
                statusUI.UpdateStatus("System Preparation (Bootstrapper)", "Starting", "Please wait...", true, -1);

                tempWorkingDirectory = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), @"Disco\Temp");
                if (!Directory.Exists(tempWorkingDirectory))
                    Directory.CreateDirectory(tempWorkingDirectory);

                // Check for Network Connectivity
                statusUI.UpdateStatus(null, "Detecting Network", "Checking network connectivity, Please wait...", true, -1);
                if (!NetworkInterop.HasNetworkConnectivity())
                {
                    statusUI.UpdateStatus(null, "Detecting Network", "No network connectivity detected, Diagnosing...", true, -1);
                    statusUI_WriteAdapterInfo();

                    if (!NetworkInterop.HasNetworkConnectivity())
                    {
                        // Check for Wireless
                        var hasWireless = (NetworkInterop.NetworkAdapters.Count(na => na.IsWireless) > 0);
                        if (hasWireless)
                        {
                            // True: Do wireless loop
                            statusUI.UpdateStatus(null, "Configuring Wireless Network", "Wireless adapter detected, Configuring...", true, -1);
                            await NetworkInterop.ConfigureWireless(cancellationToken);
                            statusUI.UpdateStatus(null, "Waiting for Wireless Network", null, true, 0);
                            for (int i = 0; i < 30; i++)
                            {
                                statusUI_WriteAdapterInfo();
                                statusUI.UpdateStatus(null, null, null, true, i);
                                await Program.SleepThread(2000, false, cancellationToken);
                                if (NetworkInterop.HasNetworkConnectivity())
                                    break;
                            }
                            if (!NetworkInterop.HasNetworkConnectivity())
                            {
                                statusUI.UpdateStatus(null, "Wireless Network Failed", "Unable to connect to the wireless network, please connect the network cable...", false);
                                await Program.SleepThread(3000, false, cancellationToken);
                            }
                        }

                        if (!NetworkInterop.HasNetworkConnectivity())
                        {
                            // Instruct user to connect network cable
                            statusUI.UpdateStatus(null, "Please connect the network cable", null);
                            for (int i = 0; i < 30; i++)
                            {
                                statusUI_WriteAdapterInfo();
                                statusUI.UpdateStatus(null, null, null, true, i);
                                await Program.SleepThread(2000, false, cancellationToken);
                                if (NetworkInterop.HasNetworkConnectivity())
                                    break;
                            }
                        }
                    }

                    if (!NetworkInterop.HasNetworkConnectivity())
                    {
                        // Client Failed
                        if (completeCallback != null)
                            await completeCallback(cancellationToken);
                        return;
                    }
                }

                Tuple<Uri, string> serverDiscovery;
                statusUI.UpdateStatus(null, "Detecting Disco ICT", "Locating Disco ICT Server, Please wait...", true, -1);
                try
                {
                    serverDiscovery = EndpointDiscovery.DiscoverServer(forcedServerUrl);
                    statusUI.UpdateStatus(null, null, $"{serverDiscovery.Item1} ({serverDiscovery.Item2})", true, -1);
                }
                catch (Exception)
                {
                    statusUI.UpdateStatus(null, null, "Failed to locate Disco ICT Server, exiting...", true, -1);
                    await Program.SleepThread(2000, false, cancellationToken);
                    throw;
                }

                // Download Client
                statusUI.UpdateStatus(null, "Downloading", "Retrieving Preparation Client, Please wait...", true, -1);
                string clientSourceLocation = Path.Combine(tempWorkingDirectory, "PreparationClient.zip");
                using (var webClient = new WebClient())
                {
                    // Don't use a proxy when downloading the Client
                    webClient.Proxy = new WebProxy();
                    webClient.Headers.Add("X-DiscoICT-Discovery", serverDiscovery.Item2);
                    try
                    {
                        webClient.DownloadFile(new Uri(serverDiscovery.Item1, "/Services/Client/PreparationClient"), clientSourceLocation);
                    }
                    catch (WebException ex)
                    {
                        if (ex.Response != null &&
                            ex.Response is HttpWebResponse response)
                        {
                            if (response.StatusCode == HttpStatusCode.BadRequest)
                            {
                                statusUI.UpdateStatus(null, "Download failed: Bad Request", response.StatusDescription, true, -1);
                                await Program.SleepThread(5000, false, cancellationToken);
                            }
                            else if (response.StatusCode == HttpStatusCode.InternalServerError)
                            {
                                statusUI.UpdateStatus(null, "Download failed: Something went wrong on the server", "Review logs for more information (Configuration > Logging)", true, -1);
                                await Program.SleepThread(5000, false, cancellationToken);
                            }
                        }
                        throw;
                    }
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
                ProcessStartInfo clientProcessStart = new ProcessStartInfo(Path.Combine(clientLocation, "Start.bat"), $"2 {Process.GetCurrentProcess().Id} {serverDiscovery.Item1}")
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
                CertificateInterop.RemoveTempCerts();

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
            }
            // End Of Loop
            if (completeCallback != null)
                await completeCallback(cancellationToken);
        }

        private void statusUI_WriteAdapterInfo()
        {

            var info = new StringBuilder();
            foreach (var na in NetworkInterop.NetworkAdapters)
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

        private void clientProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
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

    }
}
