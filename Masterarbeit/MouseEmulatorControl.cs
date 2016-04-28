using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace CarUI
{
   static class MouseEmulatorControl
   {
      [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
      private static extern void mouse_event(uint Flags, uint X, uint Y, uint Data, uint ExtraInfo);

      static double ScaleX = (ushort.MaxValue) / (double)Screen.AllScreens[0].Bounds.Width;
      static double ScaleY = (ushort.MaxValue) / (double)Screen.AllScreens[0].Bounds.Height;

      private enum MouseEvents : uint
      {
         MOVE = 0x0001,
         LEFT_DOWN = 0x0002,
         LEFT_UP = 0x0004,
         RIGHT_DOWN = 0x0008,
         RIGHT_UP = 0x0010,
         LEFT_CLICK = LEFT_DOWN | LEFT_UP,
         RIGHT_CLICK = RIGHT_DOWN | RIGHT_UP,

         ABSOLUTE_POS = 0x8000,
      }

      public static Point getWindowsCoords(double rawXCoord, double rawYCoord)
      {
         return new Point(rawXCoord * ScaleX, rawYCoord * ScaleY);

         //double xSlope;
         //double ySlope;
         //double offset;
         //double xCoord;

         //if (rawXCoord < 1920)
         //{
         //   offset = 0.0;
         //   xSlope = UInt16.MaxValue / 1920.0;
         //   ySlope = UInt16.MaxValue / 1080.0;
         //   xCoord = rawXCoord;
         //}
         //else
         //{
         //   offset = 81982.0;
         //   xSlope = 87295.0 / 2048.0;
         //   ySlope = 97050 / 1280.0;
         //   xCoord = rawXCoord - 1920.0;
         //}

         //double x = offset + (xCoord * xSlope);
         //double y = rawYCoord * ySlope;

         //return new Point(x, y);
      }


      public static void Move(double pX, double pY)
      {
         var p = getWindowsCoords(pX, pY);
         Console.WriteLine("Display Coords: " + p.X + " | " + p.Y);
         mouse_event((int)MouseEvents.MOVE | (int)MouseEvents.ABSOLUTE_POS, (uint)p.X, (uint)p.Y, 0, 0);
      }
      public static void Click(double pX, double pY)
      {
         var p = getWindowsCoords(pX, pY);
         mouse_event((int)MouseEvents.LEFT_CLICK | (int)MouseEvents.ABSOLUTE_POS, (uint)p.X, (uint)p.Y, 0, 0);
      }
      public static void DoubleClick(double pX, double pY)
      {
         Click(pX, pY);
         Thread.Sleep(50);
         Click(pX, pY);
      }
      public static void RightClick(double pX, double pY)
      {
         var p = getWindowsCoords(pX, pY);

         mouse_event((int)MouseEvents.RIGHT_CLICK | (int)MouseEvents.ABSOLUTE_POS, (uint)p.X, (uint)p.Y, 0, 0);
      }
   }
}
