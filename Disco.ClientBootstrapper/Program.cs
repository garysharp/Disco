using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace Disco.ClientBootstrapper
{
    static class Program
    {
        public static IStatus Status { get; set; }
        public static BootstrapperLoop BootstrapperLoop { get; set; }
        public static InstallLoop InstallLoop { get; set; }

        public static List<string> PostBootstrapperActions { get; set; }
        public static bool AllowUninstall { get; set; }
        public static bool ApplicationExiting { get; set; }
        public static Lazy<string> InlinePath = new Lazy<string>(() =>
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        });

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length > 0)
            {
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
                        InstallLoop = new InstallLoop(installLocation, wimImage, tempPath);
                        InstallLoop.Start(new InstallLoop.CompleteCallback(InstallComplete));
                        Application.Run();
                        return;
                    case "/uninstall":
                        AllowUninstall = true;
                        Status = new NullStatus();
                        Interop.InstallInterop.Uninstall();
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

            BootstrapperLoop = new BootstrapperLoop(Status, new BootstrapperLoop.LoopCompleteCallback(LoopComplete));
            BootstrapperLoop.Start();

            Application.Run();
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
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

        public static void LoopComplete()
        {
            // Run Post Actions
            if (PostBootstrapperActions != null)
            {
                // Check Uninstall
                if (AllowUninstall && PostBootstrapperActions.Contains("UninstallBootstrapper"))
                {
                    Interop.InstallInterop.Uninstall();
                }

                // Check ShutdownActions
                if (PostBootstrapperActions.Contains("Shutdown"))
                {
                    Status.UpdateStatus("System Preparation (Bootstrapper)", "Shutting Down; Finished...", string.Empty, false, 0);
                    SleepThread(4000, true);
                    Interop.ShutdownInterop.Shutdown();
                }
                else if (PostBootstrapperActions.Contains("Reboot"))
                {
                    Status.UpdateStatus("System Preparation (Bootstrapper)", "Rebooting; Finished...", string.Empty, false, 0);
                    SleepThread(4000, true);
                    Interop.ShutdownInterop.Reboot();
                }
                else
                {
                    Status.UpdateStatus("System Preparation (Bootstrapper)", "Starting System; Finished...", string.Empty, false, 0);
                    SleepThread(2000, true);
                }
            }
            else
            {
                Status.UpdateStatus("System Preparation (Bootstrapper)", "Starting System; Finished...", string.Empty, false, 0);
                SleepThread(2000, true);
            }

            ExitApplication();
        }

        public static void InstallComplete()
        {
            ExitApplication();
        }

        public static void ExitApplication()
        {
            if (!ApplicationExiting)
            {
                ApplicationExiting = true;
                if (BootstrapperLoop != null)
                {
                    if (BootstrapperLoop.LoopThread != null)
                    {
                        if (BootstrapperLoop.LoopThread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                        {
                            BootstrapperLoop.LoopThread.Interrupt();
                        }
                        if (BootstrapperLoop.LoopThread.ThreadState == System.Threading.ThreadState.Running)
                        {
                            BootstrapperLoop.LoopThread.Abort();
                        }
                    }
                }
                Application.Exit();
            }
        }

        public static void Trace(string Format, params string[] args)
        {
            System.Diagnostics.Debug.WriteLine(Format, args);
        }

        public static void SleepThread(int millisecondsTimeout, bool updateUI)
        {
            if (updateUI)
            {
                for (int i = 0; i < millisecondsTimeout; i += 500)
                {
                    int progress = Convert.ToInt32(((Convert.ToDouble(i) / Convert.ToDouble(millisecondsTimeout)) * 100));
                    Status.UpdateStatus(null, null, null, true, progress);
                    Thread.Sleep(500);
                }
            }
            else
            {
                Thread.Sleep(millisecondsTimeout);
            }
        }
    }
}
