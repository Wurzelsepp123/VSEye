using SPOCK.UIElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CarUI
{
    public class DasherInterface
    {
        const int c_dasherCoordPort = 20320;
        const int c_dasherRetrievalPort = 20321;
        const int c_dasherControlPort = 20319;

        const string c_appName = "Document1 - Word";

        bool m_running;
        EyeMovementCanvas m_canvas;

        string m_dasherHost;

        UdpClient m_coordClient;
        TcpClient m_letterRetrievalClient;
        TcpClient m_controlClient;
        private double yOffset = 0.00;
        private double xOffset = 0.00;
        Process[] wordProcess;
        IntPtr Wordwindow;

        public DasherInterface(EyeMovementCanvas canvas, string dasherHost)
        {
            m_canvas = canvas;
            m_dasherHost = dasherHost;
            m_coordClient = new UdpClient(m_dasherHost, c_dasherCoordPort);

           

            m_letterRetrievalClient = new TcpClient();
            Task.Run(() => startLetterRetrievClient());

            m_controlClient = new TcpClient();
            Task.Run(() => connectControlClient());
        }

        private void connectControlClient()
        {
            while (!m_controlClient.Connected)
            {
                Console.WriteLine("Try to connect control client!");
                try
                {
                    m_controlClient.Connect(m_dasherHost, c_dasherControlPort);
                }
                catch (SocketException e)
                {
                    Console.WriteLine("Cannot connect! Retrying in 1 Second!");
                    Thread.Sleep(1000);
                }
            }
        }

        public void startDasher()
        {
            if (m_running == true)
                return;
            if (!m_controlClient.Connected)
                return;

            byte[] dgram = Encoding.ASCII.GetBytes("s");
            m_controlClient.GetStream().Write(dgram, 0, 1);
            Thread.Sleep(500);

            doSendPosition(new Point(0.5, 0.5));
            Task.Run(() =>
            {
                Thread.Sleep(1600);
                m_canvas.EyeMove += SendGazeCoordsToDasher;
            });
            m_running = true;
            wordProcess = Process.GetProcessesByName("WINWORD");
            if (wordProcess.Length > 0)
            {
                Wordwindow = wordProcess[0].MainWindowHandle;
            }
        }

        public void stopDasher()
        {
            if (m_running == false)
                return;
            if (!m_controlClient.Connected)
                return;

            byte[] dgram = Encoding.ASCII.GetBytes("q");
            m_controlClient.GetStream().Write(dgram, 0, 1);
            Thread.Sleep(500);

            m_running = false;
            m_canvas.EyeMove -= SendGazeCoordsToDasher;
            doSendPosition(new Point(0.5, 0.5));
            Task.Run(() =>
            {
                Thread.Sleep(1600);
                if (m_running == false)
                    doSendPosition(new Point(0.5, 0));
            });
        }

        private void SendGazeCoordsToDasher(Object sender, EyeMovedEventArgs args)
        {
            var pos = (m_canvas.PointFromScreen(args.getPosition()));

            pos.X /= m_canvas.Width;
            pos.X = 1.0 - pos.X;

            pos.Y /= m_canvas.Height;
            doSendPosition(pos);
        }

        private void doSendPosition(Point pos)
        {
            string message;
            byte[] dgram;

            pos.X += xOffset;
            pos.Y += yOffset;

            message = "x " + pos.X + "\n";
            message = message.Replace('.', ',');
            dgram = Encoding.ASCII.GetBytes(message);
            m_coordClient.Send(dgram, dgram.Length);

            message = "y " + pos.Y + "\n";
            message = message.Replace('.', ',');
            dgram = Encoding.ASCII.GetBytes(message);
            m_coordClient.Send(dgram, dgram.Length);
        }

        void startLetterRetrievClient()
        {
            while (true)
            {
                m_letterRetrievalClient = new TcpClient();
                while (!m_letterRetrievalClient.Connected)
                {
                    Console.WriteLine("Try to connect to coord receiver!");
                    try
                    {
                        m_letterRetrievalClient.Connect(m_dasherHost, c_dasherRetrievalPort);
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine("Cannot connect! Retrying...");
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
        }

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindow(string lpszClass, string lpszWindow);
        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

       

   

        void sendStringToApplication(string text, string appName = c_appName)
        {
            //Process[] list = Process.GetProcesses();
            //foreach (var process in list)

            //{
            //    Console.Write(process.ProcessName+"\n");
            //}

           
            if (wordProcess.Length > 0)
            {
               
                //var wordProcess = Process.GetProcesses().First(x => { return x.ProcessName.Contains("WINDWORD"); });
                //IntPtr window = wordProcess.MainWindowHandle;


                //IntPtr window = FindWindow(null, appName);
                SetForegroundWindow(Wordwindow);
                try
                {
                    System.Windows.Forms.SendKeys.SendWait(text);
                }
                catch (SystemException)
                {
                    Console.Write("Invalid Sign");
                }
            }

        }
        void sendBackspaceToApplication(string appName = c_appName)
        {
         
            if (wordProcess.Length > 0)
            {
                //IntPtr window = wordProcess[0].MainWindowHandle;
                SetForegroundWindow(Wordwindow);
                System.Windows.Forms.SendKeys.SendWait("{BS}");
            }
        }
    }
}
