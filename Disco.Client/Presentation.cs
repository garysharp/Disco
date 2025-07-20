﻿using Disco.Client.Extensions;
using Disco.Client.Interop;
using System;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Disco.Client
{
    public static class Presentation
    {
        public static bool DelayUI { get; set; }

        private static string EscapeMessage(this string Message)
        {
            if (!string.IsNullOrEmpty(Message))
                return Message.Replace(",", "{comma}").Replace(Environment.NewLine, "{newline}");
            else
                return null;
        }
        public static void UpdateStatus(string SubHeading, string Message, bool ShowProgress, int Progress, int TryDelay)
        {
            UpdateStatus(SubHeading, Message, ShowProgress, Progress);
            if (TryDelay > 0)
                Presentation.TryDelay(TryDelay);
        }
        public static void UpdateStatus(string SubHeading, string Message, bool ShowProgress, int Progress)
        {
            Console.WriteLine($"#{SubHeading.EscapeMessage()},{Message.EscapeMessage()},{ShowProgress.ToString()},{Progress.ToString()}");
        }
        public static void TryDelay(int Milliseconds)
        {
            if (DelayUI)
                Thread.Sleep(Milliseconds);
        }

        public static void WriteBanner()
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine($"Version: {Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}");
            message.AppendLine($"Device: {Hardware.Information.SerialNumber} ({Hardware.Information.Manufacturer} {Hardware.Information.Model})");
            Console.ForegroundColor = ConsoleColor.Yellow;
            UpdateStatus("Preparation Client Started", message.ToString(), false, 0);
            Console.ForegroundColor = ConsoleColor.White;
            TryDelay(3000);
        }
        public static void WriteFatalError(Exception ex)
        {

            Console.ForegroundColor = ConsoleColor.Magenta;
            ClientServiceException clientServiceException = ex as ClientServiceException;
            if (clientServiceException != null)
            {
                UpdateStatus($"An error occurred during {clientServiceException.ServiceFeature}",
                    clientServiceException.Message, false, 0);
            }
            else
            {
                StringBuilder message = new StringBuilder();
                message.Append("Type: ").AppendLine(ex.GetType().Name);
                message.Append("Message: ").AppendLine(ex.Message);
                message.Append("Source: ").AppendLine(ex.Source);
                message.Append("Stack: ").AppendLine(ex.StackTrace);
                UpdateStatus("An error occurred", message.ToString(), false, 0);
            }
            Console.ForegroundColor = ConsoleColor.White;

            TryDelay(30000);
        }
        public static void WriteFooter(bool RebootRequired, bool AllowUninstall, bool ErrorEncountered)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            if (ErrorEncountered)
            {
                UpdateStatus("Client Finished due to Error", "An error occurred which caused the Preparation Client to stop running.", false, 0);
                TryDelay(5000);
            }
            else
            {
                UpdateStatus("Client Finished Successfully", "The Preparation Client finished successfully.", false, 0);
                TryDelay(1000);
            }

            if (RebootRequired)
                RegisterBootstrapperPostActions(ShutdownActions.Reboot, AllowUninstall && !ErrorEncountered);
            else
                RegisterBootstrapperPostActions(ShutdownActions.None, AllowUninstall && !ErrorEncountered);
        }

        public static void RegisterBootstrapperPostActions(ShutdownActions ShutdownAction, bool Uninstall)
        {
            Console.WriteLine($"!{Enum.GetName(typeof(ShutdownActions), ShutdownAction)},{(Uninstall ? "UninstallBootstrapper" : "DontUninstallBootstrapper")}");
        }
        public enum ShutdownActions
        {
            None,
            Shutdown,
            Reboot
        }

    }
}
