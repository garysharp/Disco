using System;
using System.Windows.Forms;

namespace Disco.ClientBootstrapper
{
    public partial class FormStatus : Form, IStatus
    {

        private delegate void dUpdateStatus(string Heading, string SubHeading, string Message, Nullable<bool> ShowProgress, Nullable<int> Progress);
        private dUpdateStatus mUpdateStatus;

        public FormStatus()
        {
            InitializeComponent();

            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            this.labelVersion.Text = $"v{version.ToString(3)}";

            this.FormClosed += new FormClosedEventHandler(FormStatus_FormClosed);

            mUpdateStatus = new dUpdateStatus(UpdateStatusDo);
            Cursor.Hide();
        }

        void FormStatus_FormClosed(object sender, FormClosedEventArgs e)
        {
            Cursor.Show();
            Program.ExitApplication();
        }

        public void UpdateStatus(string Heading, string SubHeading, string Message, Nullable<bool> ShowProgress = null, Nullable<int> Progress = null)
        {
            try
            {
                this.Invoke(mUpdateStatus, Heading, SubHeading, Message, ShowProgress, Progress);
            }
            catch (Exception) { }
        }
        private void UpdateStatusDo(string Heading, string SubHeading, string Message, Nullable<bool> ShowProgress, Nullable<int> Progress)
        {
            if (Heading != null)
                if (this.labelHeading.Text != Heading)
                    this.labelHeading.Text = Heading;
            if (SubHeading != null)
                if (this.labelSubHeading.Text != SubHeading)
                    this.labelSubHeading.Text = SubHeading;
            if (Message != null)
                if (this.labelMessage.Text != Message)
                    this.labelMessage.Text = Message;

            if (ShowProgress.HasValue)
            {
                if (ShowProgress.Value)
                {
                    this.progressBar.Visible = true;
                    if (Progress.HasValue)
                    {
                        if (Progress.Value >= 0)
                        {
                            this.progressBar.Value = Math.Min(Progress.Value, 100);
                            this.progressBar.Style = ProgressBarStyle.Continuous;
                        }
                        else
                        {
                            this.progressBar.Style = ProgressBarStyle.Marquee;
                        }
                    }
                }
                else
                {
                    this.progressBar.Visible = false;
                }
            }
        }
    }
}
