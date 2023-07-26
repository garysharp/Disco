namespace Disco.ClientBootstrapper
{
    partial class FormStatus
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelHeading = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.labelSubHeading = new System.Windows.Forms.Label();
            this.labelMessage = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelHeading
            // 
            this.labelHeading.BackColor = System.Drawing.Color.Transparent;
            this.labelHeading.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHeading.Location = new System.Drawing.Point(15, 15);
            this.labelHeading.Name = "labelHeading";
            this.labelHeading.Size = new System.Drawing.Size(270, 20);
            this.labelHeading.TabIndex = 0;
            this.labelHeading.Text = "System Preparation";
            // 
            // progressBar
            // 
            this.progressBar.BackColor = System.Drawing.Color.White;
            this.progressBar.Location = new System.Drawing.Point(15, 100);
            this.progressBar.Margin = new System.Windows.Forms.Padding(2);
            this.progressBar.MarqueeAnimationSpeed = 50;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(381, 15);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 1;
            this.progressBar.Visible = false;
            // 
            // labelSubHeading
            // 
            this.labelSubHeading.BackColor = System.Drawing.Color.White;
            this.labelSubHeading.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSubHeading.Location = new System.Drawing.Point(15, 35);
            this.labelSubHeading.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelSubHeading.Name = "labelSubHeading";
            this.labelSubHeading.Size = new System.Drawing.Size(381, 20);
            this.labelSubHeading.TabIndex = 2;
            // 
            // labelMessage
            // 
            this.labelMessage.BackColor = System.Drawing.Color.White;
            this.labelMessage.Location = new System.Drawing.Point(15, 55);
            this.labelMessage.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(381, 60);
            this.labelMessage.TabIndex = 3;
            // 
            // labelVersion
            // 
            this.labelVersion.BackColor = System.Drawing.Color.White;
            this.labelVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVersion.ForeColor = System.Drawing.Color.Gray;
            this.labelVersion.Location = new System.Drawing.Point(229, 15);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(167, 20);
            this.labelVersion.TabIndex = 0;
            this.labelVersion.Text = "Disco ICT Bootstrapper v";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // FormStatus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::Disco.ClientBootstrapper.Properties.Resources.Background_BW;
            this.ClientSize = new System.Drawing.Size(411, 130);
            this.Controls.Add(this.labelHeading);
            this.Controls.Add(this.labelSubHeading);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.labelMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormStatus";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Disco ICT - Client Bootstrapper";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.Magenta;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelHeading;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label labelSubHeading;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.Label labelVersion;
    }
}

