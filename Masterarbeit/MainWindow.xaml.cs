using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Masterarbeit
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //start starter = new start();
        public bool useVM;
        public MainWindow()
        {
            InitializeComponent();

        }
              

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            Handle(sender as CheckBox);
        }
        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Handle(sender as CheckBox);
        }

        void Handle(CheckBox checkBox)
        {
            // Use IsChecked.
            bool flag = checkBox.IsChecked.Value;
           // starter.removeErrors = flag;
        }

        private async void start_button_Click(object sender, RoutedEventArgs e)
        {
            start_button.IsEnabled = false;
            stop_button.IsEnabled = true;
            TrackingWindow.use_dasher = useVM;
            TrackingWindow win2 = new TrackingWindow();            
            //win2.use_dasher = useVM;
            win2.Show();


            //var t = Task.Run(() => starter.startTracker());
            

            //await t;
            //show_result(t.Result);
        }



        private void stop_button_Click(object sender, RoutedEventArgs e)
        {
            start_button.IsEnabled = true;
            stop_button.IsEnabled = false;
           // starter.keep_running = false;
        }

        delegate void set_result(string type);
        public void show_result(string type)
        {
            //Console.Write(type);         
            //label.Content =type;
        }

        private void chkVM_Checked(object sender, RoutedEventArgs e)
        {
            useVM = (bool)((CheckBox)sender).IsChecked;
        }
    }
}
