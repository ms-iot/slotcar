using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPControl
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
        private void Track1_Click(object sender, RoutedEventArgs e)
        {
            TrackControlInit init = new TrackControlInit { lane = TrackLane.Lane1, computerName = NetworkName.Text };
            this.Frame.Navigate(typeof(TrackControl), init);
        }

        private void Track2_Click(object sender, RoutedEventArgs e)
        {
            TrackControlInit init = new TrackControlInit { lane = TrackLane.Lane2, computerName = NetworkName.Text };
            this.Frame.Navigate(typeof(TrackControl), init);
        }
        private void RobotTrack_Click(object sender, RoutedEventArgs e)
        {
            TrackControlInit init = new TrackControlInit { lane = TrackLane.Lane1VsRobot, computerName = NetworkName.Text };
            this.Frame.Navigate(typeof(TrackControl), init);
        }
    }
}
