using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ThumbPreview
{
    class DWM
    {
        #region Imports

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);


        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmEnableBlurBehindWindow(
            IntPtr hWnd, DWM_BLURBEHIND pBlurBehind);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmExtendFrameIntoClientArea(
            IntPtr hWnd, MARGINS pMargins);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmEnableComposition(bool bEnable);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmGetColorizationColor(
            out int pcrColorization,
            [MarshalAs(UnmanagedType.Bool)]out bool pfOpaqueBlend);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern IntPtr DwmRegisterThumbnail(
            IntPtr dest, IntPtr source);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmUnregisterThumbnail(IntPtr hThumbnail);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmUpdateThumbnailProperties(
            IntPtr hThumbnail, DWM_THUMBNAIL_PROPERTIES props);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmQueryThumbnailSourceSize(
            IntPtr hThumbnail, out Size size);

        [StructLayout(LayoutKind.Sequential)]
        public class DWM_THUMBNAIL_PROPERTIES
        {
            public uint dwFlags;
            public RECT rcDestination;
            public RECT rcSource;
            public byte opacity;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fVisible;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fSourceClientAreaOnly;
            public const uint DWM_TNP_RECTDESTINATION = 0x00000001;
            public const uint DWM_TNP_RECTSOURCE = 0x00000002;
            public const uint DWM_TNP_OPACITY = 0x00000004;
            public const uint DWM_TNP_VISIBLE = 0x00000008;
            public const uint DWM_TNP_SOURCECLIENTAREAONLY = 0x00000010;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MARGINS
        {
            public int cxLeftWidth, cxRightWidth,
                       cyTopHeight, cyBottomHeight;

            public MARGINS(int left, int top, int right, int bottom)
            {
                cxLeftWidth = left; cyTopHeight = top;
                cxRightWidth = right; cyBottomHeight = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class DWM_BLURBEHIND
        {
            public uint dwFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fEnable;
            public IntPtr hRegionBlur;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fTransitionOnMaximized;

            public const uint DWM_BB_ENABLE = 0x00000001;
            public const uint DWM_BB_BLURREGION = 0x00000002;
            public const uint DWM_BB_TRANSITIONONMAXIMIZED = 0x00000004;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left, top, right, bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left; this.top = top;
                this.right = right; this.bottom = bottom;
            }
        }

        #endregion

        public static void ApplyGlass(Form form)
        {
            MARGINS m = new MARGINS(-1, -1, -1, -1);
            //MARGINS m = new MARGINS(1, 1, 1, 1);
            form.Paint += new PaintEventHandler(form_Paint);
            DwmExtendFrameIntoClientArea(form.Handle, m);
        }

        static void form_Paint(object sender, PaintEventArgs e)
        {
            SolidBrush blackBrush = new SolidBrush(Color.Black);
            e.Graphics.FillRectangle(blackBrush, e.ClipRectangle);
        }


        IntPtr _PreviewHandle = IntPtr.Zero;
        IntPtr _ThumbnailHandle = IntPtr.Zero;
        Timer _timer = null;
        Form _form;
        DWM_THUMBNAIL_PROPERTIES _Properties;
        Size _oldSize;

        int _HeightRatio = 0, _WidthRatio = 0;


        public void RegisterThumbnailPreview(Form form, IntPtr PreviewHandle)
        {

            _form = form;
            _oldSize = _form.ClientSize;


            _ThumbnailHandle = DwmRegisterThumbnail(form.Handle, PreviewHandle);

            _Properties = new DWM_THUMBNAIL_PROPERTIES();
             
            _Properties.dwFlags =
                    DWM_THUMBNAIL_PROPERTIES.DWM_TNP_VISIBLE +
                    DWM_THUMBNAIL_PROPERTIES.DWM_TNP_OPACITY +
                    DWM_THUMBNAIL_PROPERTIES.DWM_TNP_RECTDESTINATION +
                    DWM_THUMBNAIL_PROPERTIES.DWM_TNP_SOURCECLIENTAREAONLY;

            _Properties.opacity = 255;
            _Properties.fVisible = true;

            _Properties.rcSource = _Properties.rcDestination = new RECT(0, 0,
                    _form.ClientRectangle.Right, _form.ClientRectangle.Bottom);

            _Properties.fSourceClientAreaOnly = true;

            DwmUpdateThumbnailProperties(_ThumbnailHandle, _Properties);

            Size sz;
            DwmQueryThumbnailSourceSize(_ThumbnailHandle, out sz);


            _WidthRatio = sz.Width;
            _HeightRatio = sz.Height;

            _form.Width = sz.Width / 2;

            if (_form.Width < 200) _form.Width = 200;

            _form.Disposed += new EventHandler(frm_Disposed);
            _form.Resize += new EventHandler(frm_Resize);

            frm_Resize(null, null);


             _timer = new Timer();

             _timer.Interval = 1000;
             _timer.Tick += new EventHandler(t_Tick);
             _timer.Enabled = true;

        }


        void t_Tick(object sender, EventArgs e)
        {
            if (_ThumbnailHandle == IntPtr.Zero)
            {
                _timer.Enabled = false;
                _timer.Dispose();
                return;
            }

            Size sz;
            try
            {
                DwmQueryThumbnailSourceSize(_ThumbnailHandle, out sz);
            }
            catch (Exception)
            {
                _form.Close();
                return;
            }

            if (_WidthRatio != sz.Width ||
                _HeightRatio != sz.Height)
            {
                _WidthRatio = sz.Width;
                _HeightRatio = sz.Height;
                frm_Resize(null, null);
            }
        }

        void frm_Resize(object sender, EventArgs e)
        {
            if (_ThumbnailHandle != IntPtr.Zero)
            {
                _Properties.rcDestination = 
                    new RECT(0, 0, _form.ClientRectangle.Right, _form.ClientRectangle.Bottom);

                try
                {
                    DwmUpdateThumbnailProperties(_ThumbnailHandle, _Properties);
                }
                catch (Exception)
                {
                    return; // Can't do anything about it.
                }

                if (_form.ClientSize.Width == _oldSize.Width)
                {
                    _form.ClientSize = new Size((int)(_WidthRatio* (double)_form.ClientSize.Height/(double)_HeightRatio) ,_form.ClientSize.Height);
                }
                else
                {
                    _form.ClientSize = new Size(_form.ClientSize.Width,
                        (int)(_HeightRatio * (double)_form.ClientSize.Width / (double)_WidthRatio));
                }
                _oldSize = _form.ClientSize;
            }
        }

        void frm_Disposed(object sender, EventArgs e)
        {
            if (_ThumbnailHandle != IntPtr.Zero)
            {
                if (DwmIsCompositionEnabled())
                    try
                    {
                        DwmUnregisterThumbnail(_ThumbnailHandle);
                    }
                    catch (Exception) { } // shutting down
                _ThumbnailHandle = IntPtr.Zero;
            }
        }

        public Point ConvertPoint(Point point)
        {
            Debug.WriteLine("Converting " + point.X + " " + point.Y);

            Point p = new Point();
            
            p.X = (int) ((double)point.X / (double)_form.ClientRectangle.Width * (double)_WidthRatio);
            p.Y =(int) ((double)point.Y / (double)_form.ClientRectangle.Height * (double)_HeightRatio);

            Debug.WriteLine("Result: " + p.X + " " + p.Y);

            return p;
        }
    }
}