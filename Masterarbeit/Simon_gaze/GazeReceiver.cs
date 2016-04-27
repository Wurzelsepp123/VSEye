using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SPOCK.Receivers
{
   public abstract class GazeReceiver
   {
      public static GazeReceiver Instance
      {
         get { return m_Singleton; }
      }
      private static readonly GazeReceiver m_Singleton = new MouseEmulator();

      public ulong LastTimestamp
      {
         get;
         protected set;
      }

      public delegate void CalibrationDoneCB();
      abstract public void Calibrate(Canvas calibrationArea, int calibrationPoints, int fixationDuration, CalibrationDoneCB callback);
      abstract public bool IsCalibrated();
      abstract public void Init();
      abstract public void Start();
      abstract public void Stop();
      public void AddListener(IEyeMovementListener listener)
      {
         m_Llisteners.Add(listener);
      }
      public void RemoveListener(IEyeMovementListener listener)
      {
         m_Llisteners.Remove(listener);
      }

      protected ArrayList m_Llisteners = new ArrayList();
      protected GazeReceiver()
      {
         System.Windows.Forms.Application.ApplicationExit += (s, a) => { Destroy(); };
      }
      protected void updatePosition(double x, double y, ulong timestamp, double precision)
      {
         if (precision < 0.5)
            return;

         LastTimestamp = timestamp;

         for (var i = 0; i < m_Llisteners.Count; i++)
         {
            ((IEyeMovementListener)m_Llisteners[i]).RaiseEyeEvents(new Point(x, y), timestamp, precision);
         }
      }
      abstract protected void Destroy();
   }
}
