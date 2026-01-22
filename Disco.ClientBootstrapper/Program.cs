using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Disco.ClientBootstrapper
{
    internal static class Program
    {
        private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public static IStatus Status { get; private set; }

        public static List<string> PostBootstrapperActions { get; set; }
        public static bool AllowUninstall { get; private set; }
        public static Uri ForcedServerUrl { get; private set; } = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;


            if (args.Length > 0)
            {
#if DEBUG
                if (args.Any(a => a.Equals("debug", StringComparison.OrdinalIgnoreCase)))
                {
                    do
                    {
                        Console.WriteLine("Waiting for Debugger to Attach");
                        Thread.Sleep(1000);
                    } while (!System.Diagnostics.Debugger.IsAttached);
                }
#endif

                if (args.Any(a => a.StartsWith("http://", StringComparison.OrdinalIgnoreCase)))
                    throw new ArgumentException("Only HTTPS URLs are supported for a forced server URL.");
                var forcedServerArg = args.FirstOrDefault(a => a.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
                if (forcedServerArg != null)
                {
                    if (Uri.TryCreate(forcedServerArg, UriKind.Absolute, out var forcedUri))
                        ForcedServerUrl = forcedUri;
                    else
                        throw new ArgumentException("The provided forced server URL is not valid.");
                }

                switch (args[0].ToLower())
                {
                    case "/install":
                        var statusForm = new FormStatus();
                        Status = statusForm;
                        statusForm.Show();
                        string installLocation = null;
                        string wimImage = null;
                        string tempPath = null;
                        if (args.Length > 1)
                            installLocation = args[1];
                        if (args.Length > 2)
                            wimImage = args[2];
                        if (args.Length > 3)
                            tempPath = args[3];
                        var installLoop = new InstallLoop(installLocation, wimImage, tempPath, InstallComplete, ForcedServerUrl);
                        installLoop.Start();
                        Application.Run();
                        return;
                    case "/uninstall":
                        AllowUninstall = true;
                        Status = new NullStatus();
                        Task.Run(async () =>
                        {
                            await Interop.InstallInterop.Uninstall(cancellationTokenSource.Token);
                        }).Wait(cancellationTokenSource.Token);
                        return;
                    case "/allowuninstall":
                        AllowUninstall = true;
                        break;
                    default:
                        AllowUninstall = false;
                        break;
                }
            }

            if (Status == null)
            {
                var statusForm = new FormStatus();
                Status = statusForm;
                statusForm.Show();
            }

            var bootstrapperLoop = new BootstrapperLoop(Status, ForcedServerUrl, LoopComplete, cancellationTokenSource.Token);
            bootstrapperLoop.Start();

            Application.Run();
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            WriteAppError(e.Exception);
        }

        public static void WriteAppError(Exception ex)
        {
            try
            {
                string AppErrorPath = $"{System.Reflection.Assembly.GetExecutingAssembly().Location}.errors.txt";
                System.Text.StringBuilder ErrorMessage = new System.Text.StringBuilder();
                ErrorMessage.AppendLine();
                ErrorMessage.AppendLine(DateTime.Now.ToLongDateString());
                ErrorMessage.AppendLine(DateTime.Now.ToLongTimeString());
                ErrorMessage.AppendLine($"Type: {ex.GetType().FullName}");
                ErrorMessage.AppendLine($"Message: {ex.Message}");
                ErrorMessage.AppendLine($"Source: {ex.Source}");
                ErrorMessage.AppendLine($"Stack: {ex.StackTrace}");
                System.IO.File.AppendAllText(AppErrorPath, ErrorMessage.ToString());
            }
            catch (Exception) { }
        }

        public static async Task LoopComplete(CancellationToken cancellationToken)
        {
            // Run Post Actions
            if (PostBootstrapperActions != null)
            {
                // Check Uninstall
                if (AllowUninstall && PostBootstrapperActions.Contains("UninstallBootstrapper"))
                {
                    await Interop.InstallInterop.Uninstall(cancellationToken);
                }

                // Check ShutdownActions
                if (PostBootstrapperActions.Contains("Shutdown"))
                {
                    Status.UpdateStatus("System Preparation (Bootstrapper)", "Shutting Down; Finished...", string.Empty, false, 0);
                    await SleepThread(4000, true, cancellationToken);
                    Interop.ShutdownInterop.Shutdown();
                }
                else if (PostBootstrapperActions.Contains("Reboot"))
                {
                    Status.UpdateStatus("System Preparation (Bootstrapper)", "Rebooting; Finished...", string.Empty, false, 0);
                    await SleepThread(4000, true, cancellationToken);
                    Interop.ShutdownInterop.Reboot();
                }
                else
                {
                    Status.UpdateStatus("System Preparation (Bootstrapper)", "Starting System; Finished...", string.Empty, false, 0);
                    await SleepThread(2000, true, cancellationToken);
                }
            }
            else
            {
                Status.UpdateStatus("System Preparation (Bootstrapper)", "Starting System; Finished...", string.Empty, false, 0);
                await SleepThread(2000, true, cancellationToken);
            }

            ExitApplication();
        }

        public static void InstallComplete()
        {
            ExitApplication();
        }

        public static void ExitApplication()
        {
            if (!cancellationTokenSource.IsCancellationRequested)
                cancellationTokenSource.Cancel();
            Application.Exit();
        }

        public static async Task SleepThread(int millisecondsTimeout, bool updateUI, CancellationToken cancellationToken)
        {
            if (updateUI)
            {
                for (int i = 0; i < millisecondsTimeout; i += 500)
                {
                    int progress = Convert.ToInt32(((Convert.ToDouble(i) / Convert.ToDouble(millisecondsTimeout)) * 100));
                    Status.UpdateStatus(null, null, null, true, progress);
                    await Task.Delay(500, cancellationToken);
                }
            }
            else
            {
                await Task.Delay(millisecondsTimeout, cancellationToken);
            }
        }
    }
}
