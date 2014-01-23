namespace ThumbPreview
{
    partial class PreviewWindow
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreviewWindow));
            this.mnuContext = new System.Windows.Forms.ContextMenu();
            this.mnuAlwaysOnTop = new System.Windows.Forms.MenuItem();
            this.mnuHidewhenTargetHasFocus = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.mnuClosePreviewWindow = new System.Windows.Forms.MenuItem();
            this.tmrFocus = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // mnuContext
            // 
            this.mnuContext.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuAlwaysOnTop,
            this.mnuHidewhenTargetHasFocus,
            this.menuItem1,
            this.mnuClosePreviewWindow});
            // 
            // mnuAlwaysOnTop
            // 
            this.mnuAlwaysOnTop.Checked = true;
            this.mnuAlwaysOnTop.Index = 0;
            this.mnuAlwaysOnTop.Text = "&Always on top";
            this.mnuAlwaysOnTop.Click += new System.EventHandler(this.mnuAlwaysOnTop_Click);
            // 
            // mnuHidewhenTargetHasFocus
            //  
            this.mnuHidewhenTargetHasFocus.Index = 1;
            this.mnuHidewhenTargetHasFocus.Text = "&Hide when real window has focus";
            this.mnuHidewhenTargetHasFocus.Click += new System.EventHandler(this.mnuHidewhenTargetHasFocus_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 2;
            this.menuItem1.Text = "-";
            // 
            // mnuClosePreviewWindow
            // 
            this.mnuClosePreviewWindow.Index = 3;
            this.mnuClosePreviewWindow.Text = "&Close Preview Window";
            this.mnuClosePreviewWindow.Click += new System.EventHandler(this.mnuClosePreviewWindow_Click);
            // 
            // tmrFocus
            // 
            this.tmrFocus.Tick += new System.EventHandler(this.tmrFocus_Tick);
            // 
            // PreviewWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(297, 140);
            this.ControlBox = false;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(125, 125);
            this.Name = "PreviewWindow";
            this.TopMost = true;
            this.DoubleClick += new System.EventHandler(this.PreviewWindow_DoubleClick);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.PreviewWindow_KeyUp);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PreviewWindow_MouseClick);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.PreviewWindow_MouseWheel);
            this.ResumeLayout(false);

        }



        #endregion

        private System.Windows.Forms.ContextMenu mnuContext;
        private System.Windows.Forms.MenuItem mnuAlwaysOnTop;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem mnuClosePreviewWindow;
        private System.Windows.Forms.MenuItem mnuHidewhenTargetHasFocus;
        private System.Windows.Forms.Timer tmrFocus;

    }
}