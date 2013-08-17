using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;



namespace ThumbPreview
{
    class Win32
    {
        private const int WM_SCROLL = 276; // Horizontal scroll
        private const int WM_VSCROLL = 277; // Vertical scroll
        private const int SB_LINEUP = 0; // Scrolls one line up
        private const int SB_LINELEFT = 0;// Scrolls one cell left
        private const int SB_LINEDOWN = 1; // Scrolls one line down
        private const int SB_LINERIGHT = 1;// Scrolls one cell right
        private const int SB_PAGEUP = 2; // Scrolls one page up
        private const int SB_PAGELEFT = 2;// Scrolls one page left
        private const int SB_PAGEDOWN = 3; // Scrolls one page down
        private const int SB_PAGERIGTH = 3; // Scrolls one page right
        private const int SB_PAGETOP = 6; // Scrolls to the upper left
        private const int SB_LEFT = 6; // Scrolls to the left
        private const int SB_PAGEBOTTOM = 7; // Scrolls to the upper right
        private const int SB_RIGHT = 7; // Scrolls to the right
        private const int SB_ENDSCROLL = 8; // Ends scroll

        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;  



        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        static extern bool PostMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);


        [DllImport("user32.dll")]
        static extern IntPtr ChildWindowFromPointEx(IntPtr hWndParent, Point pt, uint uFlags);

        [Flags]
        public enum ChildWindowFromPointFlags : uint
        {
            CWP_ALL = 0x0000,
            CWP_SKIPINVISIBLE = 0x0001,
            CWP_SKIPDISABLED = 0x0002,
            CWP_SKIPTRANSPARENT = 0x0004
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator System.Drawing.Point(POINT p)
            {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator POINT(System.Drawing.Point p)
            {
                return new POINT(p.X, p.Y);
            }
        }

        const int WM_MOUSEMOVE  = 0x0200;

        [DllImport("user32.dll", SetLastError = true)]

        public static extern int MapWindowPoints(IntPtr hwndFrom, IntPtr hwndTo, ref POINT lpPoints, [MarshalAs(UnmanagedType.U4)] int cPoints);


        public static void ScrollUp(HwndPoint hp, int n)
        {
            for (int i = 0; i < n; i++)
            {
                ScrollUp(hp);
            }
        }

        public static void ScrollUp(HwndPoint hp)
        {
            hp = LocateLowestWindow(hp);

            SendMessage(hp.Hwnd, WM_VSCROLL,(IntPtr) SB_LINEUP, IntPtr.Zero);
        }

        public static void ScrollDown(HwndPoint hp, int n)
        {
            for (int i = 0; i < n; i++)
            {
                ScrollDown(hp);
            }
        }

        public static void ScrollDown(HwndPoint hp)
        {
            hp = LocateLowestWindow(hp);

            SendMessage(hp.Hwnd, WM_VSCROLL, (IntPtr) SB_LINEDOWN, IntPtr.Zero);
        }

        private static HwndPoint LocateLowestWindow(HwndPoint hp)
        {

            Debug.WriteLine("LocateWindow: " + hp.Hwnd);
            IntPtr ch = ChildWindowFromPointEx(hp.Hwnd, new Point(hp.X, hp.Y), (System.UInt32)ChildWindowFromPointFlags.CWP_ALL);
            Debug.WriteLine("Child Result: " + ch);
            if (ch == hp.Hwnd)
            {
                return hp;
            }
            else if (ch == null)
            {
                return hp;
            }
            else
            {
                // ch is a child window - dig deeper

                POINT[] pnt = new POINT[1];
                pnt[0].X = hp.X;
                pnt[0].Y = hp.Y;
                Debug.WriteLine("Point x " + pnt[0].X + " y " + pnt[0].Y);
                MapWindowPoints(hp.Hwnd , ch, ref pnt[0], pnt.Length);
                Debug.WriteLine("Point x " + pnt[0].X + " y " + pnt[0].Y);

                hp.X = pnt[0].X;
                hp.Y = pnt[0].Y;
                hp.Hwnd = ch;

                return LocateLowestWindow(hp);
            }
        }

        public static void MouseMove(HwndPoint hp)
        {
            hp = LocateLowestWindow(hp);
            Debug.WriteLine("Posting WM_MOUSEMOVE to " + hp.Hwnd + " at location " + hp.X  + " " + hp.Y);
            PostMessage(hp.Hwnd, WM_MOUSEMOVE, (IntPtr)0, (IntPtr)MakeLParam(hp.X, hp.Y));
        }

        public static void MouseLeftClick(HwndPoint hp)
        {
            hp = LocateLowestWindow(hp);
            Debug.WriteLine("Sending WM_LBUTTONDOWN to " + hp.Hwnd + " at location " + hp.X + " " + hp.Y);
            SendMessage(hp.Hwnd, WM_LBUTTONDOWN, (IntPtr)0, (IntPtr)MakeLParam(hp.X, hp.Y));
            SendMessage(hp.Hwnd, WM_LBUTTONUP, (IntPtr)0, (IntPtr)MakeLParam(hp.X, hp.Y));
        }

        private static int MakeLParam(int LoWord, int HiWord)
        {
            return (int)((HiWord << 16) | (LoWord & 0xffff));
        }


  
    }
    public class HwndPoint
    {

        private IntPtr _hwnd;

        public IntPtr Hwnd
        {
            get { return _hwnd; }
            set { _hwnd = value; }
        }

        private int _x;

        public int X
        {
            get { return _x; }
            set { _x = value; }
        }

        private int _y;

        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }
        

        public HwndPoint(IntPtr hwnd, int x, int y)
        {
            _hwnd = hwnd;
            _x = x;
            _y = y;
        }
    }
}
