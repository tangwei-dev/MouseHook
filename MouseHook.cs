using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MouseHook
{
    [StructLayout(LayoutKind.Sequential)]
    public class POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class MouseHookStruct
    {
        public POINT pt;
        public int hwnd;
        public int wHitTestCode;
        public int dwExtraInfo;
    }

    public enum MouseButton
    {
        Left,
        Right
    }

    public class MouseHook
    {
        private Point Point { get; set; }

        IntPtr hHook;
        public const int WH_MOUSE = 7;
        public const int WH_MOUSE_LL = 14;
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_LBUTTONDOWN = 0x0201;
        private Win32Api.HookProc hProc;

        public MouseHook()
        {
            this.Point = new Point();
        }
        public bool SetHook()
        {
            hProc = new Win32Api.HookProc(MouseHookProc);
            using (ProcessModule curModule = Process.GetCurrentProcess().MainModule)
            {
                hHook = Win32Api.SetWindowsHookEx(WH_MOUSE_LL, hProc, Kernel32.GetModuleHandle(curModule.ModuleName), 0);
            }

            return hHook != IntPtr.Zero;
        }
        public bool UnHook()
        {
            bool retMouse = true;
            if (hHook != IntPtr.Zero)
            {
                retMouse = Win32Api.UnhookWindowsHookEx(hHook);
                hHook = IntPtr.Zero;
            }

            if (!retMouse)
            {
                Console.WriteLine("UnhookWindowsHookEx failed.");
                return false;
            }
            return true;
        }

        private IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            MouseHookStruct MyMouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
            if (nCode < 0)
            {
                return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
            else
            {
                switch ((int)wParam)
                {
                    case (int)WM_LBUTTONDOWN:
                        this.Point = new Point(MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y);
                        MouseClickEvent?.Invoke(MouseButton.Left, this.Point);
                        break;
                    case (int)WM_RBUTTONDOWN:
                        this.Point = new Point(MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y);
                        MouseClickEvent?.Invoke(MouseButton.Right, this.Point);
                        break;
                    case (int)WM_MOUSEMOVE:
                        Point point = new Point(MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y);
                        if (this.Point != point)
                        {
                            this.Point = point;
                            MouseMoveEvent?.Invoke(point);
                        }
                        break;
                    default:
                        break;
                }
                return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
        }
        public event Action<Point> MouseMoveEvent;
        public event Action<MouseButton, Point> MouseClickEvent;
    }
}
