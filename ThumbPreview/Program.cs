using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace ThumbPreview
{
    static class Program
    {
        static KeyboardHook hook;
        static NotifyIcon nicon;
        static ContextMenu mnu;

        static MenuItem _sep;

        static LinkedList<PreviewWindow> windows = new LinkedList<PreviewWindow>();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);
        

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(Point Point);


        public static string GetText(IntPtr hWnd)
        {
            //return "TEST";

            //.MenuItem.int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(255);
            GetWindowText(hWnd,  sb, sb.Capacity);
            return sb.ToString();
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //  DavuxLib.Registration.CanExecuteEx("ThumbPreview", "", true);

            try
            {
                if (!DWM.DwmIsCompositionEnabled())
                {
                    DialogResult ret = MessageBox.Show("Desktop Window Composition (Aero) is not enabled.  Would you like to enable it?", "DWM is not enabled", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (ret == DialogResult.Yes)
                    {
                        DWM.DwmEnableComposition(true);

                        if (!DWM.DwmIsCompositionEnabled())
                        {
                            MessageBox.Show("The DWM could not be enabled.  This must be corrected before running this application.", "Problem enabling DWM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Application.Exit();
                        }

                    }
                    else
                    {
                        Application.Exit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem with the Desktop Window Manager:\n\n" + ex.Message, "There was a problem with the DWM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            try
            {
                hook = new KeyboardHook();
                hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
                hook.RegisterHotKey(KBModifierKeys.Control | KBModifierKeys.Shift, Keys.Z);
                //hook.RegisterHotKey(KBModifierKeys.Win | KBModifierKeys.Shift, Keys.Z);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not register keyboard shortcut (Win+Z).", "There was a problem creating a keyboard shortcut", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }



            mnu = new ContextMenu();

             _sep = new MenuItem("-");
            _sep.Visible = false;
            mnu.MenuItems.Add(_sep);
            mnu.MenuItems.Add("&About", new EventHandler(mnuAbout_Click));
            mnu.MenuItems.Add(new MenuItem("-"));
            mnu.MenuItems.Add("E&xit", new EventHandler(mnuExit_Click));

            nicon = new NotifyIcon();
            nicon.Visible = true;
            nicon.Icon = new System.Drawing.Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ThumbPreview.Resources.AppIcon.ico"));
            nicon.Text = "Movable Thumbnail Preview";
            nicon.ContextMenu = mnu;
            nicon.Click += new EventHandler(nicon_Click);

            Application.Run();
        }

        static void mnuExit_Click(object sender, EventArgs e)
        {
            nicon.Dispose();
            Application.Exit();
        }

        static void mnuAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Movable Preview - By David Amenta - DaveAmenta@gmail.com - www.DaveAmenta.com","About Movable Preview",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

        static void nicon_Click(object sender, EventArgs e)
        {
            foreach (PreviewWindow win in windows)
            {
                if (win != null)
                {
                    if (win.Visible)
                    {
                        Debug.WriteLine("Showing (loop): " + win.Handle.ToString());
                        BringWindowToTop(win.Handle);

                        if (!win.TopMost)
                        {
                            win.TopMost = true;
                            Application.DoEvents();
                            win.TopMost = false;
                        }
                    }
                }
            }
        }


        static void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            
            IntPtr pWin = IntPtr.Zero;
            /*
            if (e.Key == Keys.Z && e.Modifier == (KBModifierKeys.Win | KBModifierKeys.Shift))
            {
                Debug.WriteLine("Got WinShiftZ");
                Point p = new Point();
                bool hr = GetCursorPos(out p);

                if (hr)
                {
                    Debug.WriteLine("Got Cursor Pos");
                    pWin = WindowFromPoint(p);
                    if (pWin == IntPtr.Zero)
                    {
                        Debug.WriteLine("WindowFromPoint Failed");
                        pWin = GetForegroundWindow();
                    }
                }
                else
                {
                    pWin = GetForegroundWindow();
                }

            }
            else
            {
                Debug.WriteLine("Got WinZ");
                pWin = GetForegroundWindow();
            }
            */

            pWin = GetForegroundWindow();

            if (pWin == IntPtr.Zero)
            {
                Debug.WriteLine("Bad Window - Not Adding");
                return;
            }

            string title = GetText(pWin);



            PreviewWindow win = new PreviewWindow(pWin);

            win.Disposed += new EventHandler(win_Disposed);

            MenuItem mi = new MenuItem();

            mi.Tag = win;
            mi.Text = "Window: " + title;
            mi.Click += new EventHandler(mi_Click);

            mnu.MenuItems.Add(0, mi);


            windows.AddLast(win);
            Debug.WriteLine("Adding Window... ");
            _sep.Visible = true;
        }

        static void mi_Click(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;

            if (item != null)
            {
                PreviewWindow win = (PreviewWindow)item.Tag;

                if (win != null)
                {
                    win.KIllTimer();
                    Debug.WriteLine("Showing (click): " + win.Handle.ToString());
                    BringWindowToTop(win.Handle);
                    
                }
            }

        }



        static void win_Disposed(object sender, EventArgs e)
        {
            PreviewWindow win = (PreviewWindow)sender;
            windows.Remove(win);

            for (int i = 0; i < mnu.MenuItems.Count; i++)
            {
                if (mnu.MenuItems[i].Tag == (object)win)
                {
                    mnu.MenuItems.RemoveAt(i);
                    break;
                }
            }

            if (windows.Count == 0)
            {
                _sep.Visible = false;
            }


            Debug.WriteLine("Removing Window...");
        }
    }
}
