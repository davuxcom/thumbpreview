using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ThumbPreview
{
    public partial class PreviewWindow : Form
    {
        [DllImport("user32.dll")]
        static extern bool GetAsyncKeyState(System.Windows.Forms.Keys vKey);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        IntPtr _PreviewHandle = IntPtr.Zero;

        DWM dwm;

        public PreviewWindow(IntPtr PreviewHandle)
        {
            InitializeComponent();

            _PreviewHandle = PreviewHandle;

            // enable dragging
            new FormDrag(this);

            // enable glass
            DWM.ApplyGlass(this);

            try
            {
                dwm = new DWM();

                dwm.RegisterThumbnailPreview(this, PreviewHandle);
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem requesting the thumbnail preview:\n\n" + ex.Message);
                Close();
                return;
            }
            Show();


        }



        private void PreviewWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }



        private void PreviewWindow_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                mnuContext.Show(this, e.Location);
            }

            /*
            else
            {
                bool controlPressed = GetAsyncKeyState(Keys.ControlKey);

                if (controlPressed)
                {
                    Point p = dwm.ConvertPoint(new Point(e.X, e.Y));

                    Win32.MouseLeftClick(new HwndPoint(_PreviewHandle, e.X, e.Y));
                }
            }
             */
        }
        void PreviewWindow_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Delta == 0)
            {
                return;
            }
            //bool controlPressed = GetAsyncKeyState(Keys.ControlKey);

            if (false)
            {
                if (e.Delta > 0)
                {
                    Win32.ScrollUp(new HwndPoint(_PreviewHandle, e.X, e.Y), Math.Abs(e.Delta));
                }
                else
                {
                    Win32.ScrollDown(new HwndPoint(_PreviewHandle, e.X, e.Y), Math.Abs(e.Delta));
                }
            }
            else
            {
                this.Width += e.Delta/3;
            }
        }

        private void mnuAlwaysOnTop_Click(object sender, EventArgs e)
        {
             mnuAlwaysOnTop.Checked = this.TopMost = !this.TopMost;
        }

        private void mnuClosePreviewWindow_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void PreviewWindow_MouseMove(object sender, MouseEventArgs e)
        {

            
            bool controlPressed = GetAsyncKeyState(Keys.ControlKey);

            if (controlPressed)
            {

                Point p = dwm.ConvertPoint(new Point(e.X, e.Y));

                Win32.MouseMove(new HwndPoint(_PreviewHandle, e.X, e.Y));
            }
        }

        private void PreviewWindow_DoubleClick(object sender, EventArgs e)
        {
            ShowWindow(_PreviewHandle, 9);
            BringWindowToTop(_PreviewHandle);
        }

        private void PreviewWindow_Load(object sender, EventArgs e)
        {

        }

        private void mnuHidewhenTargetHasFocus_Click(object sender, EventArgs e)
        {
            mnuHidewhenTargetHasFocus.Checked = !mnuHidewhenTargetHasFocus.Checked;

            if (mnuHidewhenTargetHasFocus.Checked)
            {
                tmrFocus.Interval = 250;
                tmrFocus.Enabled = true;
            }
            else
            {
                tmrFocus.Enabled = false;
            }
        }

        public void KIllTimer()
        {
            tmrFocus.Enabled = false;
            mnuHidewhenTargetHasFocus.Checked = false;
        }


        private void tmrFocus_Tick(object sender, EventArgs e)
        {
            IntPtr pfwin = GetForegroundWindow();

            if (pfwin == _PreviewHandle)
            {
                if (this.Visible)
                {
                    this.Hide();
                    this.Visible = false;
                }
            }
            else
            {
                if (!this.Visible)
                {
                    this.Show();
                    this.Visible = true;
                }
            }
        }


    }
}
