namespace SignMyDickInstaller
{
    partial class MainWindow
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.lblBrowser = new System.Windows.Forms.Label();
            this.tbBrowserPath = new System.Windows.Forms.TextBox();
            this.ckAutoReinstall = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbLog = new System.Windows.Forms.TextBox();
            this.btnInstall = new System.Windows.Forms.Button();
            this.btnUninstall = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.BrowserFinderDialog = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(478, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(29, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lblBrowser
            // 
            this.lblBrowser.AutoSize = true;
            this.lblBrowser.Location = new System.Drawing.Point(12, 17);
            this.lblBrowser.Name = "lblBrowser";
            this.lblBrowser.Size = new System.Drawing.Size(48, 13);
            this.lblBrowser.TabIndex = 1;
            this.lblBrowser.Text = "Browser:";
            // 
            // tbBrowserPath
            // 
            this.tbBrowserPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbBrowserPath.Location = new System.Drawing.Point(66, 14);
            this.tbBrowserPath.Name = "tbBrowserPath";
            this.tbBrowserPath.ReadOnly = true;
            this.tbBrowserPath.Size = new System.Drawing.Size(406, 20);
            this.tbBrowserPath.TabIndex = 2;
            // 
            // ckAutoReinstall
            // 
            this.ckAutoReinstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ckAutoReinstall.AutoSize = true;
            this.ckAutoReinstall.Checked = true;
            this.ckAutoReinstall.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckAutoReinstall.Location = new System.Drawing.Point(407, 40);
            this.ckAutoReinstall.Name = "ckAutoReinstall";
            this.ckAutoReinstall.Size = new System.Drawing.Size(100, 17);
            this.ckAutoReinstall.TabIndex = 3;
            this.ckAutoReinstall.Text = "Auto Reinstaller";
            this.ckAutoReinstall.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(28, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Log:";
            // 
            // tbLog
            // 
            this.tbLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLog.Location = new System.Drawing.Point(15, 57);
            this.tbLog.Multiline = true;
            this.tbLog.Name = "tbLog";
            this.tbLog.ReadOnly = true;
            this.tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbLog.Size = new System.Drawing.Size(492, 155);
            this.tbLog.TabIndex = 5;
            this.tbLog.WordWrap = false;
            // 
            // btnInstall
            // 
            this.btnInstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInstall.Enabled = false;
            this.btnInstall.Location = new System.Drawing.Point(435, 218);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(72, 23);
            this.btnInstall.TabIndex = 6;
            this.btnInstall.Text = "Install";
            this.btnInstall.UseVisualStyleBackColor = true;
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            // 
            // btnUninstall
            // 
            this.btnUninstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUninstall.Enabled = false;
            this.btnUninstall.Location = new System.Drawing.Point(357, 218);
            this.btnUninstall.Name = "btnUninstall";
            this.btnUninstall.Size = new System.Drawing.Size(72, 23);
            this.btnUninstall.TabIndex = 7;
            this.btnUninstall.Text = "Uninstall";
            this.btnUninstall.UseVisualStyleBackColor = true;
            this.btnUninstall.Click += new System.EventHandler(this.btnUninstall_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(279, 218);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(72, 23);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // BrowserFinderDialog
            // 
            this.BrowserFinderDialog.DefaultExt = "exe";
            this.BrowserFinderDialog.Filter = "Executable Files|*.exe";
            this.BrowserFinderDialog.Title = "Select the Browser Main Executable";
            this.BrowserFinderDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.BrowserFinderDialog_FileOk);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(519, 253);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnUninstall);
            this.Controls.Add(this.btnInstall);
            this.Controls.Add(this.tbLog);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ckAutoReinstall);
            this.Controls.Add(this.tbBrowserPath);
            this.Controls.Add(this.lblBrowser);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MainWindow";
            this.Text = "SignMyDick Installer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblBrowser;
        private System.Windows.Forms.TextBox tbBrowserPath;
        private System.Windows.Forms.CheckBox ckAutoReinstall;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbLog;
        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.Button btnUninstall;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.OpenFileDialog BrowserFinderDialog;
    }
}

