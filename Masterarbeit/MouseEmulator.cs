using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Diagnostics;

namespace SPOCK.Receivers
{
    public class MouseEmulator : EyeReceiver
    {
        //       public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        //       private int hHook = 0;
        //       const int WH_MOUSE_LL = 14;
        //       const int WH_MOUSE = 7;
        //       HookProc CallbackMethod;

        Thread workerThread;
        bool running = false;


        public MouseEmulator()
        {
            workerThread = new Thread(mainLoop);
            workerThread.Name = "MouseReceiverMainLoop";
            workerThread.Start();
        }
        ~MouseEmulator()
        {
            workerThread.Abort();
        }
        public static double averageMS;

        void mainLoop()
        {
            try
            {
                long delay = (long)((double)TimeSpan.TicksPerMillisecond * 0.30);
                long frameCount = 0;
                Stopwatch busyWaitingSW = new Stopwatch();
                Stopwatch sw = new Stopwatch();

                //while (true)
                //{
                //   UInt64 unixTimestamp = (UInt64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
                //   Point p = GetCursorPosition();
                //   if (running)
                //      updatePosition(p.X, p.Y, unixTimestamp, 1.0);

                //   frameCount++;
                //   averageMS = (double)sw.ElapsedMilliseconds / (double)frameCount;
                //   if (frameCount == 10000)
                //   {
                //      frameCount = 0;
                //      sw.Restart();
                //   }
                //   busyWaitingSW.Start();
                //   while (busyWaitingSW.ElapsedTicks < delay)
                //   { }
                //   busyWaitingSW.Reset();


                //}

                //TEst STUFF
                while (true)
                {
                    UInt64 unixTimestamp = (UInt64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
                    Point p = GetCursorPosition();
                    if (running)
                    {
                        updatePosition(p.X, p.Y, unixTimestamp, 1.0);
                    }
                    busyWaitingSW.Start();
                    while (busyWaitingSW.ElapsedMilliseconds < 16)
                    { }
                    busyWaitingSW.Reset();
                }


            }
            catch (ThreadAbortException)
            { return; }
        }

        override protected void Destroy()
        {
        }
        public override void Init()
        {
        }
        override public void Calibrate(Canvas calibrationArea, int count, int fixationDuration, CalibrationDoneCB callback)
        {
            Task.Run(() => { Thread.Sleep(500); callback(); });
        }
        override public bool IsCalibrated()
        {
            return true;
        }

        public override void Start()
        {
            running = true;
        }

        public override void Stop()
        {
            running = false;
        }

        #region ActivePolling
        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            //bool success = User32.GetCursorPos(out lpPoint);
            // if (!success)

            return lpPoint;
        }
        #endregion

        /*
        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
           public int x;
           public int y;
           public int hwnd;
           public int wHitTestCode;
           public int dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        public class MouseMoveStruct
        {

        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);


        private int mouseListener(int nCode, IntPtr wParam, IntPtr lParam)
        {
            MouseHookStruct s = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
            if (nCode < 0)
                return CallNextHookEx(hHook, nCode, wParam, lParam);

            UInt64 unixTimestamp = (UInt64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            if (running)
                updatePosition(s.x, s.y, unixTimestamp, 100.0);

            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }
        */
    }
}
