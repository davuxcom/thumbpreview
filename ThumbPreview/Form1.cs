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
    public partial class Form1 : Form
    {

        KeyboardHook hook = new KeyboardHook();

        private IntPtr m_hThumbnail;
        //double so division keeps decimal points

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("dwmapi.dll")]
        static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

         double widthRatio = 1;
         double heightRatio = 1;

        const int WM_SIZING = 0x214;
        const int WMSZ_LEFT = 1;
        const int WMSZ_RIGHT = 2;
        const int WMSZ_TOP = 3;
        const int WMSZ_BOTTOM = 6;

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }


        public Form1()
        {
            InitializeComponent();
            
            hook.KeyPressed +=
            new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);

            hook.RegisterHotKey(KBModifierKeys.Win, Keys.Z);

            MARGINS m = new MARGINS();
            m.bottomHeight = -1;
            m.leftWidth = -1;
            m.rightWidth = -1;
            m.topHeight = -1;

            

            DwmExtendFrameIntoClientArea(Handle, ref m);

            //this.RecreateHandle();
        }

        void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            //CreateAndShow(new IntPtr(0x000603C4));
            CreateAndShow(GetForegroundWindow());
        }

        public void CreateAndShow(IntPtr sourceWindow)
        {
            try
            {
                m_hThumbnail = DwmApi.DwmRegisterThumbnail(
                    Handle, sourceWindow);

                DwmApi.DWM_THUMBNAIL_PROPERTIES m_ThumbnailProperties =
                    new DwmApi.DWM_THUMBNAIL_PROPERTIES();
                m_ThumbnailProperties.dwFlags =
                    DwmApi.DWM_THUMBNAIL_PROPERTIES.DWM_TNP_VISIBLE +
                    DwmApi.DWM_THUMBNAIL_PROPERTIES.DWM_TNP_OPACITY +
                    DwmApi.DWM_THUMBNAIL_PROPERTIES.DWM_TNP_RECTDESTINATION +
                    DwmApi.DWM_THUMBNAIL_PROPERTIES.
                        DWM_TNP_SOURCECLIENTAREAONLY;
                m_ThumbnailProperties.opacity = 255;
                m_ThumbnailProperties.fVisible = true;
                m_ThumbnailProperties.rcSource =
                    m_ThumbnailProperties.rcDestination = new DwmApi.RECT(0, 0,
                        ClientRectangle.Right, ClientRectangle.Bottom);
                m_ThumbnailProperties.fSourceClientAreaOnly = true;

                DwmApi.DwmUpdateThumbnailProperties(
                    m_hThumbnail, m_ThumbnailProperties);
                Size sz;
                DwmApi.DwmQueryThumbnailSourceSize(m_hThumbnail, out sz);


                widthRatio = sz.Width;
                heightRatio = sz.Height;

                this.Width = 400;
                this.Height = 400;


                Show();
            }
            catch (Exception ex)
            {
               // label1.Text = ex.Message;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);

            if (m_hThumbnail != IntPtr.Zero)
            {
                if (DwmApi.DwmIsCompositionEnabled())
                    try
                    {
                        DwmApi.DwmUnregisterThumbnail(m_hThumbnail);
                    }
                    catch (Exception ex)
                    {
                        // FIXME: handle this
                    }
                m_hThumbnail = IntPtr.Zero;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (m_hThumbnail != IntPtr.Zero)
            {
                DwmApi.DWM_THUMBNAIL_PROPERTIES m_ThumbnailProperties =
                    new DwmApi.DWM_THUMBNAIL_PROPERTIES();
                m_ThumbnailProperties.dwFlags =
                    DwmApi.DWM_THUMBNAIL_PROPERTIES.DWM_TNP_VISIBLE +
                    DwmApi.DWM_THUMBNAIL_PROPERTIES.DWM_TNP_OPACITY +
                    DwmApi.DWM_THUMBNAIL_PROPERTIES.DWM_TNP_RECTDESTINATION +
                    DwmApi.DWM_THUMBNAIL_PROPERTIES.
                        DWM_TNP_SOURCECLIENTAREAONLY;
                m_ThumbnailProperties.opacity = 255;
                m_ThumbnailProperties.fVisible = true;
                m_ThumbnailProperties.rcSource =
                    m_ThumbnailProperties.rcDestination = new DwmApi.RECT(0, 0,
                        ClientRectangle.Right, ClientRectangle.Bottom);
                m_ThumbnailProperties.fSourceClientAreaOnly = true;

                DwmApi.DwmUpdateThumbnailProperties(
                    m_hThumbnail, m_ThumbnailProperties);


                ClientSize = new Size(ClientSize.Width , (int)(heightRatio * (double)ClientSize.Width / (double)widthRatio));

            }
        }

        /*
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SIZING)
            {
                RECT rc = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));
                int res = m.WParam.ToInt32();
                if (res == WMSZ_LEFT || res == WMSZ_RIGHT)
                {
                    //Left or right resize -> adjust height (bottom)
                    rc.Bottom = rc.Top + (int)(heightRatio * Width / widthRatio);
                }
                else if (res == WMSZ_TOP || res == WMSZ_BOTTOM)
                {
                    //Up or down resize -> adjust width (right)
                    rc.Right = rc.Left + (int)(widthRatio * Height / heightRatio);
                }
                else if (res == WMSZ_RIGHT + WMSZ_BOTTOM)
                {
                    //Lower-right corner resize -> adjust height (could have been width)
                    rc.Bottom = rc.Top + (int)(heightRatio * Width / widthRatio);
                }
                else if (res == WMSZ_LEFT + WMSZ_TOP)
                {
                    //Upper-left corner -> adjust width (could have been height)
                    rc.Left = rc.Right - (int)(widthRatio * Height / heightRatio);
                }
                Marshal.StructureToPtr(rc, m.LParam, true);
            }

            base.WndProc(ref m);
        }
        */
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private bool isDragging = false;
        private int oldX, oldY;

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            oldX = e.X;
            oldY = e.Y;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Top = Top + (e.Y - oldY);

               Left = Left + (e.X - oldX);
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            SolidBrush blackBrush = new SolidBrush(Color.Black);
            e.Graphics.FillRectangle(blackBrush, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
        }

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            Form1_MouseDown(sender, e);
        }

        private void label1_MouseUp(object sender, MouseEventArgs e)
        {
            Form1_MouseUp(sender, e);
        }

        private void label1_MouseMove(object sender, MouseEventArgs e)
        {
            Form1_MouseMove(sender, e);
        }



 
    }


    
}
