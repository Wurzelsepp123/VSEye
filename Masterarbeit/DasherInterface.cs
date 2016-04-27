using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TestDasherScket
{
   public class DasherInterface
   {
      const int c_dasherControlPort = 20320;
      const int c_dasherRetrievalPort = 12345;

      bool m_running;
      Canvas m_canvas;

      const string c_appName = "Document1 - Word";

      UdpClient m_letterSendClient;
      TcpClient m_letterRetrievalClient;
      public DasherInterface(Canvas canvas, string dasherHost)
      {
         m_canvas = canvas;
            m_letterSendClient = new UdpClient(dasherHost, c_dasherControlPort);
         m_letterRetrievalClient = new TcpClient();

         Task.Run(() => startTcpClient(dasherHost));
      }

      public void startDasher()
      {
         if (m_running == true)
            return;
         doSendPosition(new Point(0.5, 0.5));
         Task.Run(() =>
         {
            Thread.Sleep(1600);
            m_canvas.MouseMove += SendGazeCoordsToDasher;
         });
         m_running = true;
      }

      public void stopDasher()
      {
         if (m_running == false)
            return;
         m_running = false;
         m_canvas.MouseMove -= SendGazeCoordsToDasher;
         doSendPosition(new Point(0.5, 0.5));
         Task.Run(() =>
         {
            Thread.Sleep(1600);
            if (m_running == false)
               doSendPosition(new Point(0.5, 0));
         });
      }

      private void SendGazeCoordsToDasher(Object sender, MouseEventArgs args)
      {
         var pos = (args.GetPosition(sender as UIElement));
         pos.X /= m_canvas.Width;
         pos.Y /= m_canvas.Height;
         doSendPosition(pos);
      }

      private void doSendPosition(Point pos)
      {
         string message;
         byte[] dgram;

         message = "x " + pos.X + "\n";
         message = message.Replace('.', ',');
         dgram = Encoding.ASCII.GetBytes(message);
            m_letterSendClient.Send(dgram, dgram.Length);

         message = "y " + pos.Y + "\n";
         message = message.Replace('.', ',');
         dgram = Encoding.ASCII.GetBytes(message);
            m_letterSendClient.Send(dgram, dgram.Length);
      }

      void startTcpClient(string hostname)
      {
         while (!m_letterRetrievalClient.Connected)
         {
            Console.WriteLine("Try to connect to Server!");
            try
            {
               m_letterRetrievalClient.Connect(hostname, c_dasherRetrievalPort);
            }
            catch (SocketException e)
            {
               Console.WriteLine("Cannot connect! Retrying in 1 Second!");
            }
         }
         Console.WriteLine("Connected to server!");
         var stream = m_letterRetrievalClient.GetStream();

         int length = 0;
         do
         {
            byte[] buffer = new byte[64];
            try
            {
               length = stream.Read(buffer, 0, buffer.Length);
            }
            catch (IOException)
            {
               break;
            }
            if (length != 0 && buffer[0] == 8)
            {
               Console.WriteLine("Received Data: Backspace");
               sendBackspaceToApplication();
            }
            else
            {
               string message = Encoding.ASCII.GetString(buffer);
               message = message.TrimEnd('\0');
               Console.WriteLine("Received Data: " + message);
               sendStringToApplication(message);
            }
         } while (length != 0);
      }

      [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
      public static extern IntPtr FindWindow(string lpszClass, string lpszWindow);
      [DllImport("User32.dll")]
      public static extern bool SetForegroundWindow(IntPtr hWnd);

      void sendStringToApplication(string text, string appName = c_appName)
      {
         //IntPtr window = FindWindow(null, appName);
         //SetForegroundWindow(window);
         //System.Windows.Forms.SendKeys.SendWait(text);
      }
      void sendBackspaceToApplication(string appName = c_appName)
      {
         //IntPtr window = FindWindow(null, appName);
         //SetForegroundWindow(window);
         //System.Windows.Forms.SendKeys.SendWait("{BS}");
      }
   }
}
