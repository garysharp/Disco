using System;
using System.Windows.Forms;

namespace Disco.ClientBootstrapper
{
    public partial class FormStatus : Form, IStatus
    {

        private delegate void dUpdateStatus(string Heading, string SubHeading, string Message, bool? ShowProgress, int? Progress);
        private readonly dUpdateStatus mUpdateStatus;

        public FormStatus()
        {
            InitializeComponent();

            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            labelVersion.Text = $"v{version.ToString(3)}";

            FormClosed += new FormClosedEventHandler(FormStatus_FormClosed);

            mUpdateStatus = new dUpdateStatus(UpdateStatusDo);
            Cursor.Hide();
        }

        private void FormStatus_FormClosed(object sender, FormClosedEventArgs e)
        {
            Cursor.Show();
            Program.ExitApplication();
        }

        public void UpdateStatus(string Heading, string SubHeading, string Message, bool? ShowProgress = null, int? Progress = null)
        {
            try
            {
                Invoke(mUpdateStatus, Heading, SubHeading, Message, ShowProgress, Progress);
            }
            catch (Exception) { }
        }
        private void UpdateStatusDo(string Heading, string SubHeading, string Message, bool? ShowProgress, int? Progress)
        {
            if (Heading != null)
                if (labelHeading.Text != Heading)
                    labelHeading.Text = Heading;
            if (SubHeading != null)
                if (labelSubHeading.Text != SubHeading)
                    labelSubHeading.Text = SubHeading;
            if (Message != null)
                if (labelMessage.Text != Message)
                    labelMessage.Text = Message;

            if (ShowProgress.HasValue)
            {
                if (ShowProgress.Value)
                {
                    progressBar.Visible = true;
                    if (Progress.HasValue)
                    {
                        if (Progress.Value >= 0)
                        {
                            progressBar.Value = Math.Min(Progress.Value, 100);
                            progressBar.Style = ProgressBarStyle.Continuous;
                        }
                        else
                        {
                            progressBar.Style = ProgressBarStyle.Marquee;
                        }
                    }
                }
                else
                {
                    progressBar.Visible = false;
                }
            }
        }
    }
}
