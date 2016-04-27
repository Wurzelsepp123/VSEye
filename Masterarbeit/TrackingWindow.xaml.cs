using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Sockets;
using SPOCK.Receivers;
using SPOCK.UIElements;
using SPOCK.Classifiers;
using SPOCK.Controls;
using System.Windows.Media.Animation;
using TestDasherScket;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Masterarbeit
{
    /// <summary>
    /// Interaction logic for TrackingWindow.xaml
    /// </summary>
    public partial class TrackingWindow : Window
    {
        GazeReceiver m_Receiver = GazeReceiver.Instance;
        sp_classifier m_classifier = sp_classifier.Instance;

        Ellipse gazeDot = new Ellipse()
        {
            Width = 10,
            Height = 10,
            Fill = Brushes.Blue,
        };
        TcpClient m_controlClient;
        DasherInterface di;
        int threshold;
        public bool use_dasher;

        public TrackingWindow()
        {
            InitializeComponent();
            System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.SustainedLowLatency;
            KeyDown += (sender, args) =>
            {
                if (args.Key == Key.Escape)
                    Exit();
            };






            InitializeComponent();

            if (use_dasher)
            {
                m_controlClient = new TcpClient();
                while (!m_controlClient.Connected)
                {
                    Console.WriteLine("Try to connect to socket!");


                    try
                    {
                        m_controlClient.Connect("192.168.248.128", 20319);
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine("Cannot connect control! Retrying in 1 Second!");
                    }

                    //Thread.Sleep(1000);
                }
                Console.WriteLine("Connected");
                di = new DasherInterface(MainCanvas, "192.168.248.1");
                di.startDasher();
            }


            Left = 0;
            Top = 0;

            MainCanvas.Width = Width;
            MainCanvas.Height = Height;
            threshold = 100;


            KeyDown += InitScreenKeyDown;

            FocusManager.SetFocusedElement(this, ParticipantID_textBox);
            gazeDot.Visibility = Visibility.Visible;
            Cursor = Cursors.None;


            //SPOCKButton newButton = new SPOCKButton(new Point(800, 500), 200, 200, Brushes.RosyBrown, 2);
            //MainCanvas.Children.Add(newButton);





            //MainCanvas.EyeMove += (sender, args) =>
            //{
            //    if (sender != MainCanvas)
            //        return;
            //    if (!MainCanvas.Children.Contains(gazeDot))
            //        MainCanvas.Children.Add(gazeDot);
            //    gazeDot.Fill = Brushes.Red;

            //    if (m_classifier.Result == SPOCKClassifier.Gesture.SPU)
            //        gazeDot.Fill = Brushes.Red;
            //    else if (m_classifier.Result == SPOCKClassifier.Gesture.SPD)
            //        gazeDot.Fill = Brushes.Green;
            //    else
            //        gazeDot.Fill = Brushes.Blue;
            //};


            m_Receiver.Init();
            MainCanvas.EyeMove += setDot;
            MainCanvas.MouseUp += MouseClicked;
           // MainCanvas.MouseDown += MouseClicked;



            //MainCanvas.EyeMove += DefocusSPCKButton;



            Thread workerThread;
            workerThread = new Thread(SPeverywhere);
            workerThread.Name = "DwellTimeThread";
            workerThread.IsBackground = true;
            workerThread.Start();



        }


        private void InitScreenKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.L:
                    var dia = new System.Windows.Forms.OpenFileDialog()
                    {
                        //InitialDirectory = System.IO.Path.GetDirectoryName(m_expPath),
                        //Filter = "XML-Files | *.xml",
                        //Multiselect = false,
                    };
                    //if (dia.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    // m_expPath = dia.FileName;
                    break;
                case Key.F1:
                    // StartExperiment();
                    m_Receiver.Start();
                    break;

                case Key.F2:
                    // m_skipCalibration = true;
                    break;

                case Key.F5:
                    //StartBenchmark();
                    break;
                default:
                    break;
            }
        }

        private void SPeverywhere()
        {
            int cnt = 0;
            bool activeDwell = false;
            Point startPosDwell = new Point(0, 0);
            while (true)
            {
                //Stopwatch busyWaitingSW = new Stopwatch();
                //busyWaitingSW.Start();
                //while (busyWaitingSW.ElapsedMilliseconds < 16)
                //{ }
                //busyWaitingSW.Reset();


                //detect dwellTime (500ms)
                if (sp_classifier.Instance.Result == sp_classifier.Gesture.FIX && activeDwell == false)
                {
                    startPosDwell = new Point(sp_classifier.Instance.curPos.x, sp_classifier.Instance.curPos.y);
                    activeDwell = true;
                    cnt++;
                }
                else if (sp_classifier.Instance.Result == sp_classifier.Gesture.FIX && activeDwell)
                {
                    cnt++;
                }
                else
                {
                    cnt = 0;
                    activeDwell = false;
                }

                //dwell detected
                if ((cnt >= 5) && ((startPosDwell - new Point(sp_classifier.Instance.curPos.x, sp_classifier.Instance.curPos.y)).Length < 10))
                {
                    //Dispatcher.Invoke(() =>
                    //{
                    //    SPOCKButton newButton = new SPOCKButton(startPosDwell, 200, 200, Brushes.RosyBrown, 2);
                    //    MainCanvas.Children.Add(newButton);
                    //});
                    if (startPosDwell.X > 0 && startPosDwell.X < 19200 && startPosDwell.Y > 0 && startPosDwell.Y < 1200)
                    {

                    }
                    Dispatcher.Invoke(() => spawnButton(startPosDwell));

                    //Dispatcher.Invoke(() => removeButton());

                    cnt = 0;
                    activeDwell = false;

                }
                Thread.Sleep(100);
            }
        }
        private void spawnButton(Point Position)
        {
            Position.X -= 100;
            Position.Y -= 100;
            SPOCKButton newButton = new SPOCKButton(Position, 200, 200, Brushes.RosyBrown, 2);
            newButton.Name = "DwellButton";
            newButton.EyeLeave += KillSPCKButton;
            var SpockBox = (UIElement)LogicalTreeHelper.FindLogicalNode(MainCanvas, "DwellButton");

            if (SpockBox == null)
            {

                MainCanvas.Children.Add(newButton);

            }





        }

        private void removeButton()
        {
            var SpockBox = (UIElement)LogicalTreeHelper.FindLogicalNode(MainCanvas, "DwellButton");
            MainCanvas.Children.Remove(SpockBox);
        }

        //private void DefocusSPCKButton(object sender, EyeMovedEventArgs a)
        //{
        //    if (sender is SPOCKButton)
        //    {

        //        //if (SPOCKButton.FocusedButton == null)
        //        //    return;

        //        //if (SPOCKButton.FocusedButton.EyeOn)
        //        //    return;

        //        //Point buttonPosition = SPOCKButton.FocusedButton.Position;
        //        //var buttonWidth = SPOCKButton.FocusedButton.Width;
        //        //var buttonHeight = SPOCKButton.FocusedButton.Height;

        //        //Point newPosition = a.getPosition();

        //        //if (Math.Abs(buttonPosition.X - newPosition.X) > threshold)
        //        //    removeButton();
        //        //else if (Math.Abs(newPosition.X - (buttonPosition.X + buttonWidth)) > threshold)
        //        //    removeButton();

        //        //if (Math.Abs(buttonPosition.Y - newPosition.Y) > threshold)
        //        //    removeButton();
        //        //else if (Math.Abs(newPosition.Y - (buttonPosition.Y + buttonWidth)) > threshold)
        //        //    removeButton();

        //        removeButton();

        //    }
        //}

        private void KillSPCKButton(object sender, EyeLeftEventArgs a)
        { removeButton(); }




        private void setDot(object sender, EyeMovedEventArgs e)
        {

            //gazeDot.tr
            //gazeDot.Height= e.getPosition().X;
            //gazeDot.Width = e.getPosition().Y;

            if (sender != MainCanvas)
                return;
            if (!MainCanvas.Children.Contains(gazeDot))
                MainCanvas.Children.Add(gazeDot);





            switch (sp_classifier.Instance.Result)
            {
                case sp_classifier.Gesture.SPD:
                    gazeDot.Fill = Brushes.Green;
                    break;
                case sp_classifier.Gesture.SPU:
                    gazeDot.Fill = Brushes.Blue;
                    break;
                case sp_classifier.Gesture.FIX:
                    gazeDot.Fill = Brushes.Red;
                    break;
                case sp_classifier.Gesture.SAC:
                    gazeDot.Fill = Brushes.Black;
                    break;
                default:
                    gazeDot.Fill = Brushes.AntiqueWhite;
                    break;

            }

            var top = Canvas.GetTop(gazeDot);
            var left = Canvas.GetLeft(gazeDot);
            //TranslateTransform trans = new TranslateTransform();
            //gazeDot.RenderTransform = trans;
            Canvas.SetLeft(gazeDot, e.getPosition().X);
            Canvas.SetTop(gazeDot, e.getPosition().Y);
            //DoubleAnimation anim1 = new DoubleAnimation(0, e.getPosition().Y - 0, TimeSpan.FromSeconds(10));
            //DoubleAnimation anim2 = new DoubleAnimation(0, e.getPosition().X - 0, TimeSpan.FromSeconds(10));
            //trans.BeginAnimation(TranslateTransform.XProperty, anim1);
            //trans.BeginAnimation(TranslateTransform.YProperty, anim2);
        }
        private void MouseClicked(object s, MouseButtonEventArgs a)
        {            
            if (a.Source is SPOCK.Controls.SPOCKButton)
            {
                if (use_dasher)
                {
                    if (a.ChangedButton == MouseButton.Left)
                    {
                        ParticipantID_textBox.Text = "Left";
                        Console.WriteLine("S");
                        if (m_controlClient.Connected)
                        {

                            byte[] dgram = Encoding.ASCII.GetBytes("s");
                            m_controlClient.GetStream().Write(dgram, 0, 1);
                        }
                    }
                    if (a.ChangedButton == MouseButton.Right)
                    {
                        ParticipantID_textBox.Text = "Right";
                        Console.WriteLine("Q");
                        if (m_controlClient.Connected)
                        {
                            byte[] dgram = Encoding.ASCII.GetBytes("q");
                            m_controlClient.GetStream().Write(dgram, 0, 1);
                        }
                    }
                }

                if (a.ChangedButton == MouseButton.Left)
                {
                    ParticipantID_textBox.Text = "Left";
                    Point ULCornerButton = ((SPOCKButton)a.Source).Position;
                    long xButtonCenter = (long)ULCornerButton.X + 100;
                    long yButtonCenter = (long)ULCornerButton.Y + 100;
                    doubleClick(xButtonCenter, yButtonCenter);
                }

                if (a.ChangedButton == MouseButton.Right)
                {
                    ParticipantID_textBox.Text = "Right";
                   
                }
            }



        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]

        private static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;

        private const int MOUSEEVENTF_LEFTUP = 0x04;

        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;

        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        void doubleClick(long x, long y)
        {
            x = -x;
            Console.Write("Double Click on: {0},{1}", x, y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
           


        }




        private void Exit(bool writeLogFile = true)
        {
            //m_Receiver.Stop();

            //if (writeLogFile)
            //{
            //    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(m_logOutputPath));
            //    File.WriteAllText(m_logOutputPath, m_logfile);
            //}

            Close();
            Environment.Exit(0);
        }






    }
}
