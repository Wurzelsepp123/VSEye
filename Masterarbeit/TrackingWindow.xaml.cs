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
//using SPOCK.Classifiers;
using SPOCK.Controls;
using System.Windows.Media.Animation;
using CarUI;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Masterarbeit
{
    /// <summary>
    /// Interaction logic for TrackingWindow.xaml
    /// </summary>
    public partial class TrackingWindow : Window
    {
        static Size[] ScreenSizes = Screen.AllScreens.Select(x => { return new Size(x.Bounds.Width, x.Bounds.Height); }).ToArray();
        double MouseX = 1900;
        double MouseY = 00.0;

        const int indexMainScreen = 0;
        const int indexGazeScreen = 2;
        const int indexWorkingScreen = 1;

        const int dasherHeight = 500;


        //GazeReceiver m_Receiver = GazeReceiver.Instance;
        EyeReceiver m_Receiver = EyeReceiver.Instance;
        sp_classifier m_classifier = sp_classifier.Instance;


        Window m_dasherCoverWindow = new Window()
        {
            //Only works if height of mainScreen==workingscreen else top has offset
            Width = Screen.AllScreens[indexWorkingScreen].Bounds.Width,
            Height = dasherHeight,
            Left = ScreenSizes[indexMainScreen].Width,
            //Top = Screen.AllScreens[indexWorkingScreen].Bounds.Height - dasherHeight,
            Top = Screen.AllScreens[indexMainScreen].Bounds.Height - dasherHeight,
            Background = Brushes.Black,
            Topmost = true,
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
        };

        Ellipse gazeDot = new Ellipse()
        {
            Width = 10,
            Height = 10,
            Fill = Brushes.Blue,
        };

        DasherInterface di;

        static public bool use_dasher;

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        public TrackingWindow()
        {
            InitializeComponent();
            System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.SustainedLowLatency;
            Closed += (_a, _b) => Environment.Exit(0);



            Process[] VMProcess = Process.GetProcessesByName("vmplayer");
            if (VMProcess.Length > 0)
            {
                IntPtr VMwindow = VMProcess[0].MainWindowHandle;
                int offset_toolbar = 50;
                MoveWindow(VMwindow, Screen.AllScreens[indexMainScreen].Bounds.Width + Screen.AllScreens[indexWorkingScreen].Bounds.Width, Screen.AllScreens[indexGazeScreen].Bounds.Height - (dasherHeight+ offset_toolbar), Screen.AllScreens[indexGazeScreen].Bounds.Width,dasherHeight+ offset_toolbar, true);
            }

            SizeChanged += (sender, args) =>
            {
                MainCanvas.Width = args.NewSize.Width;
                MainCanvas.Height = args.NewSize.Height;
            };





            DasherCanvas.Background = new SolidColorBrush() { Color = Colors.White, Opacity = 0.01 };

            this.Top = 0;
            this.Left = ScreenSizes[indexMainScreen].Width + ScreenSizes[indexWorkingScreen].Width;


            this.Width = ScreenSizes[indexGazeScreen].Width;
            this.Height = ScreenSizes[indexWorkingScreen].Height;


            DasherCanvas.Width = ScreenSizes[indexGazeScreen].Width;
            DasherCanvas.Height = dasherHeight;
            DasherCanvas.RenderTransform = new TranslateTransform(0, Height - dasherHeight);

            Canvas.SetZIndex(MainCanvas, 10000);

            di = new DasherInterface(DasherCanvas, "192.168.248.128");



           

            m_Receiver.Start();

            Ellipse gazeDot2 = new Ellipse()
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Red,
            };

            // KeyDown += InitScreenKeyDown;
            KeyDown += (sender, args) =>
            {
                switch (args.Key)
                {
                    case Key.Enter:
                        EyeReceiver.Instance.Calibrate(MainCanvas, 9, 1500, () =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                MainCanvas.Background = Brushes.Black;
                                EyeReceiver.Instance.Start();

                                
                                MainCanvas.EyeMove += (_s, _a) =>
                                {
                                    var p = MainCanvas.PointToScreen(_a.getPosition());
                                    gazeDot2.RenderTransform = new TranslateTransform(p.X, p.Y);
                                    if (!MainCanvas.Children.Contains(gazeDot2))
                                        MainCanvas.Children.Add(gazeDot2);
                                };
                                SwitchToDesktop();
                            });
                        });
                        break;
                    case Key.D1:
                        SwitchToDesktop();
                        break;
                    case Key.D2:
                        SwitchToWordMainMenu();
                        break;
                    case Key.D3:
                        SwitchToWordWriting();
                        break;
                    case Key.Escape:
                        SwitchToDesktop();
                        break;
                    case Key.D4:
                        EnableDasher();
                        break;
                        //spawn spbutton on dt target
                    case Key.D5:
                      
                            MainCanvas.EyeMove += (_s, _a) =>
                         {
                             var p = MainCanvas.PointToScreen(_a.getPosition());
                             gazeDot2.RenderTransform = new TranslateTransform(p.X, p.Y);
                             if (!MainCanvas.Children.Contains(gazeDot2))
                                 MainCanvas.Children.Add(gazeDot2);
                         };



                        MainCanvas.EyeEnter += startDT2SP;
                        MainCanvas.EyeLeave += startDT2SP;
                        break;

                    //case Key.Left:
                    //   MouseX -= Multiplyer;
                    //   MouseX = Math.Max(0, MouseX);
                    //   break;
                    //case Key.Right:
                    //   MouseX += Multiplyer;
                    //   break;
                    //case Key.Down:
                    //   MouseY += Multiplyer;
                    //   break;
                    //case Key.Up:
                    //   MouseY -= Multiplyer;
                    //   MouseY = Math.Max(0, MouseY);
                    //   break;
                    //case Key.RightShift:
                    //   MouseEmulator.Move(MouseX, MouseY);
                    //   break;
                    case Key.Q:
                        this.Close();
                        break;
                }
                Console.WriteLine(MouseX + " | " + MouseY);
            };



            //FocusManager.SetFocusedElement(this, ParticipantID_textBox);
            // gazeDot.Visibility = Visibility.Visible;

            //DasherCanvas.EyeMove += setDot;
            //MainCanvas.EyeMove += setDot;
            //MainCanvas.MouseUp += MouseClicked;
            // MainCanvas.MouseDown += MouseClicked;



            //MainCanvas.EyeMove += DefocusSPCKButton;
                         
            



        }
       
        void SwitchToDesktop()
        {
            DisableDasher();
            MainCanvas.Children.Clear();

            var ButtonBrush = new SolidColorBrush()
            {
                Color = Colors.LightGray,
                Opacity = 0.5
            };

            var but = new DTButton(new Point(120, 120), 80, 100, ButtonBrush, 1000);
            but.Click += EmulateDoubleClickOnWorkingScreen;
            but.Click += (a, b) => SwitchToWordMainMenu();

            MainCanvas.Children.Add(but);

        }
        private void EnableDasher()
        {
            m_dasherCoverWindow.Show();
            MainCanvas.Height = ScreenSizes[indexGazeScreen].Height - dasherHeight;
            di.startDasher();
        }

        private void DisableDasher()
        {
            MainCanvas.Height = ScreenSizes[indexGazeScreen].Height;
            m_dasherCoverWindow.Hide();
            di.stopDasher();
        }


        void SwitchToWordMainMenu()
        {
            DisableDasher();
            MainCanvas.Children.Clear();

            double width = 137.0;
            double height = 211.0;

            double[] columns = new double[] { 131.0, 315.0, 500.0 };
            double[] rows = new double[] { 121.0, 350.0, 578.0, 808.0, 1036.0 };

            foreach (var col in columns)
            {
                foreach (var row in rows)
                {
                    var but = newDoubleClickDTButton(col, row, width, height);
                    but.Click += (a, b) => SwitchToWordWriting();
                    MainCanvas.Children.Add(but);
                }
               (MainCanvas.Children[0] as Canvas).Background = Brushes.Red;
                (MainCanvas.Children[1] as Canvas).Background = Brushes.Green;
                (MainCanvas.Children[2] as Canvas).Background = Brushes.Blue;
            }

        }
        private void SwitchToWordWriting()
        {
            EnableDasher();
            MainCanvas.Children.Clear();

            var but1 = new GazeButton(new Point(0, 0), 30, 20, null);
            but1.Click += (a, b) => { EmulateClickOnWorkingScreen(a, b); };
            var but2 = new GazeButton(new Point(32, 0), 30, 20, null);
            but2.Click += (a, b) => { EmulateClickOnWorkingScreen(a, b); };
            var but3 = new GazeButton(new Point(64, 0), 30, 20, null);
            but3.Click += (a, b) => { EmulateClickOnWorkingScreen(a, b); };
            List<GazeButton> buttons = new List<GazeButton> { but1, but2, but3 };

            ButtonGroup group = new ButtonGroup(new Point(1820, 0), 94, 50, buttons, new Rect(1500, 30, 400, 200), MainCanvas);
            MainCanvas.Children.Add(group);
        }

        private DTButton newDoubleClickDTButton(double x, double y, double width, double height)
        {
            var buttonBrush = new SolidColorBrush()
            {
                Color = Colors.White,
                Opacity = 1,
            };
            var res = new DTButton(new Point(x, y), width, height, buttonBrush, 1000);
            res.Click += EmulateDoubleClickOnWorkingScreen;
            return res;
        }
        private void EmulateDoubleClickOnWorkingScreen(object sender, RoutedEventArgs args)
        {
            var but = sender as EyeMovementCanvas;

            Console.Write(but.PointToScreen(new Point(but.Width / 2, but.Height / 2)));
            var pos = PointToWorkingScreen(but.PointToScreen(new Point(but.Width / 2, but.Height / 2)));//but.GetScreenPosition(but.Midpoint, MainCanvas));
            //doubleClick((int)pos.X, (int)pos.Y+120);
            MouseEmulatorControl.Move(pos.X, pos.Y);
            MouseEmulatorControl.DoubleClick(pos.X, pos.Y);
        }
        private void EmulateClickOnWorkingScreen(object sender, RoutedEventArgs args)
        {
            var but = sender as EyeMovementCanvas;
            var pos = PointToWorkingScreen(but.PointToScreen(new Point(but.Width / 2, but.Height / 2)));//but.GetScreenPosition(but.Midpoint, MainCanvas));

            MouseEmulatorControl.Move(pos.X, pos.Y);
            MouseEmulatorControl.Click(pos.X, pos.Y);
        }

        private Point PointToWorkingScreen(Point pointOnGazeScreen)
        {
            return new Point((2 * (ScreenSizes[indexMainScreen].Width + ScreenSizes[indexWorkingScreen].Width)) - pointOnGazeScreen.X, pointOnGazeScreen.Y+120);
        }



        private volatile bool _shouldStop;

        private void startDT2SP(object sender, RoutedEventArgs e)
        {           
            Thread workerThread;
            if (e is EyeEnteredEventArgs)
            {
                _shouldStop = false;
                workerThread = new Thread(DT2SPthread);
                workerThread.Name = "DwellTimeThread";
                workerThread.IsBackground = true;
                workerThread.Start();
            }
            else if (e is EyeLeftEventArgs)
                _shouldStop = true;
          
        }
        private void DT2SPthread()
        {
            int cnt = 0;
            bool activeDwell = false;
           
            Point startPosDwell = new Point(0, 0);
            while (!_shouldStop)
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
                    
                        Dispatcher.Invoke(() => spawnButton(startPosDwell));
                    //spawnButton(startPosDwell);


                    //Dispatcher.Invoke(() => removeButton());

                    cnt = 0;
                    activeDwell = false;

                }
                Thread.Sleep(100);
            }
        }
        private void spawnButton(Point Position)
        {       

            var ButtonBrush = new SolidColorBrush()
            {
                Color = Colors.RosyBrown,
                Opacity = 0.5
            };
            //Point pos = PointFromScreen(Position);
            Position.X =1920-(Position.X- (Screen.AllScreens[indexMainScreen].Bounds.Width + Screen.AllScreens[indexWorkingScreen].Bounds.Width));
            //Place button on cursor pos
            Position.X -= 40;
            Position.Y -= 50;
            var but = new SPOCKButton(Position, 80, 100, ButtonBrush, 5);
            but.Name="DwellButton";
            but.EyeLeave += KillSPCKButton;
            var CurBut = (UIElement)LogicalTreeHelper.FindLogicalNode(MainCanvas, "DwellButton");
            but.Click += EmulateDoubleClickOnWorkingScreen;
            //but.Click += (a, b) => SwitchToWordMainMenu();
            
            
            if(CurBut==null)
            MainCanvas.Children.Add(but);



        }

        private void removeButton()
        {
            var SpockBox = (UIElement)LogicalTreeHelper.FindLogicalNode(MainCanvas, "DwellButton");
            MainCanvas.Children.Remove(SpockBox);
        }


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


            Canvas.SetLeft(gazeDot, e.getPosition().X);
            Canvas.SetTop(gazeDot, e.getPosition().Y);

        }
        private void MouseClicked(object s, MouseButtonEventArgs a)
        {
            if (a.Source is SPOCK.Controls.SPOCKButton)
            {
                if (use_dasher)
                {
                    if (a.ChangedButton == MouseButton.Left)
                    {

                    }
                    if (a.ChangedButton == MouseButton.Right)
                    {

                        Console.WriteLine("Q");

                    }
                }

                if (a.ChangedButton == MouseButton.Left)
                {

                    Point ULCornerButton = ((SPOCKButton)a.Source).Position;
                    int xButtonCenter = (int)ULCornerButton.X + 100;
                    int yButtonCenter = (int)ULCornerButton.Y + 100;
                    doubleClick(xButtonCenter, yButtonCenter);
                }

                if (a.ChangedButton == MouseButton.Right)
                {


                }
            }
        }





        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]

        private static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;

        private const int MOUSEEVENTF_LEFTUP = 0x04;

        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;

        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);
        internal struct INPUT
        {
            public UInt32 Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }
        internal struct MOUSEINPUT
        {
            public Int32 X;
            public Int32 Y;
            public UInt32 MouseData;
            public UInt32 Flags;
            public UInt32 Time;
            public IntPtr ExtraInfo;
        }




        void doubleClick(int x, int y)
        {            
            Console.Write("Double Click on: {0},{1}", x, y);

            var inputMouseDown = new INPUT();
            inputMouseDown.Type = 0; /// input type mouse
            inputMouseDown.Data.Mouse.Flags = 0x0002; /// left button down
            //inputMouseDown.Data.Mouse.X = x;
            //inputMouseDown.Data.Mouse.Y = y;

            var inputMouseUp = new INPUT();
            inputMouseUp.Type = 0; /// input type mouse
            inputMouseUp.Data.Mouse.Flags = 0x0004; /// left button up
            //inputMouseUp.Data.Mouse.X = x;
            //inputMouseUp.Data.Mouse.Y = y;

            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(x, y);

            var inputs = new INPUT[] { inputMouseDown, inputMouseUp, inputMouseDown, inputMouseUp };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(-x, y);
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
static class UIListExtension
{
    public static Point GetScreenPosition(this EyeMovementCanvas element, Point Position, Canvas MainCanvas)
    {
        var parantCanvas = VisualTreeHelper.GetParent(element);
        if (parantCanvas.Equals(MainCanvas))
            return (parantCanvas as Canvas).PointToScreen(Position);

        var par = parantCanvas as EyeMovementCanvas;

        return par.GetScreenPosition(new Point(par.Position.X + element.Position.X, par.Position.Y + element.Position.Y), MainCanvas);
    }
}

