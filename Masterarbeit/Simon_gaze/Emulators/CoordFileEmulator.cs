using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace SPOCK.Receivers
{
   public class CoordFileEmulator : GazeReceiver
   {
      Queue<double[]> m_values = new Queue<double[]>();

      bool m_calibrated = false;
      double m_resolutionFactor = 1.0;
      override public void Calibrate(Canvas calibrationArea, int calibrationPoints, int fixationDuration, CalibrationDoneCB callback)
      {
         OpenFileDialog dia = new OpenFileDialog();
         dia.Filter = "Coord-File|*.coord";
         dia.InitialDirectory = @"C:\Users\sim\Desktop\Results";// Environment.CurrentDirectory;
         dia.Multiselect = false;
         if (dia.ShowDialog() == DialogResult.OK)
         {
            string[] lines = System.IO.File.ReadAllLines(dia.FileName);
            for (int i = 0; i < lines.Length; i++)
            {
               string line = lines[i];
               if ('#' == line[0])
                  continue;
               else if ('g' == line[0])
               {
                  if (line.Substring(0, 4) == "gaze")
                  {
                     string[] temp = line.Split(' ');
                     double recordedWidth = Convert.ToDouble(temp[1]);
                     double recordedHeight = Convert.ToDouble(temp[2]);

                     Rectangle resolution = Screen.PrimaryScreen.Bounds;

                     m_resolutionFactor = Math.Min((double)resolution.Size.Width / recordedWidth, (double)resolution.Size.Height / recordedHeight);
                  }
               }
               else
               {
                  string[] temp = line.Split(' ');

                  double x = Convert.ToDouble(temp[1], CultureInfo.InvariantCulture) * m_resolutionFactor;
                  double y = Convert.ToDouble(temp[2], CultureInfo.InvariantCulture) * m_resolutionFactor;
                  double reliability = Convert.ToDouble(temp[3], CultureInfo.InvariantCulture);

                  m_values.Enqueue(new double[3] { x, y, reliability });
               }
            }
         }
         m_calibrated = true;
         callback();
      }

      override public bool IsCalibrated()
      {
         return m_calibrated;
      }

      override public void Init()
      {
      }
      public static double averageMS;
      bool m_running = false;
      override public void Start()
      {
         if (!m_calibrated)
            return;
         long delay = (long)((double)TimeSpan.TicksPerMillisecond * 0.30);
         m_running = true;
         Thread t = new Thread(() =>
         {
            long frameCount = 0;
            Stopwatch busyWaitingSW = new Stopwatch();
            Stopwatch sw = new Stopwatch();
            while (m_values.Count != 0 && m_running)
            {
               sw.Start();
               double[] data = m_values.Dequeue();
               if (data == null)
                  continue;
               updatePosition(data[0], data[1], 0, data[2]);
               frameCount++;
               averageMS = (double)sw.ElapsedMilliseconds / (double)frameCount;
               if (frameCount == 10000)
               {
                  frameCount = 0;
                  sw.Restart();
               }
               busyWaitingSW.Start();
               while (busyWaitingSW.ElapsedTicks < delay)
               { }
               busyWaitingSW.Reset();
            }
         });
         t.Priority = ThreadPriority.Highest;
         t.Start();
      }

      override public void Stop()
      {
         m_running = false;
      }

      protected override void Destroy()
      {
      }
   }
}
