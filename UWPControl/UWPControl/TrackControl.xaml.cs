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

using Windows.Networking;
using Windows.Networking.Sockets;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.Diagnostics;
using Windows.Devices.Sensors;
using Windows.UI.Core;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace UWPControl
{
    public enum TrackLane
    {
        Lane1,
        Lane2,
        Lane1VsRobot
    };

    public class TrackControlInit
    {
        public TrackLane lane;
        public string computerName;
    }
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TrackControl : Page
    {
        StreamSocket clientSocket = new StreamSocket();
        DataWriter writer = null;
        bool connected = false;

        HostName serverHost;

        const string gamePort1 = "25666";
        const string gamePort2 = "25667";

        public TrackControl()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var init = e.Parameter as TrackControlInit;
            progress.IsActive = true;
            Accelerometer.GetDefault().ReadingChanged += Accelerometer_ReadingChanged;

            serverHost = new HostName(init.computerName);

            if (init != null)
            {
                string port = gamePort2;
                if (init.lane == TrackLane.Lane1)
                {
                    port = gamePort1;
                }

                try
                {
                    await clientSocket.ConnectAsync(serverHost, port);
                    writer = new DataWriter(clientSocket.OutputStream);
                    ContentRoot.Visibility = Visibility.Visible;

                    connected = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);

                    ConnectionError.Visibility = Visibility.Visible;
                }

                progress.IsActive = false;
                progress.Visibility = Visibility.Collapsed;

                if (!connected)
                {
                    await Task.Delay(5000);
                    this.Frame.Navigate(typeof(MainPage));
                }

            }

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Accelerometer.GetDefault().ReadingChanged -= Accelerometer_ReadingChanged;
            if (clientSocket != null)
            {
                clientSocket.Dispose();
                clientSocket = null;
            }

            if (writer != null)
            {
                writer.Dispose();
                writer = null;
            }
        }

        private async void Accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        {
            double speed = (1.0 - args.Reading.AccelerationX);
            var i = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                /*
                // Show the values graphically.
                xLine.X2 = xLine.X1 + args.Reading.AccelerationX * 200;
                yLine.Y2 = yLine.Y1 - args.Reading.AccelerationY * 200;
                zLine.X2 = zLine.X1 - args.Reading.AccelerationZ * 100;
                zLine.Y2 = zLine.Y1 + args.Reading.AccelerationZ * 100;
                */

               // Speed.Value =  speed * Speed.Maximum;
            });

            if (connected)
            {
                // Scale number to 0 - .75;
                double pwm = (speed * 0.75) * 255.0;
                try
                {
                    string s = "{\"PWM\": \"" + pwm.ToString("0") + "\"}\n";
                    uint len = writer.WriteString(s);
                    await writer.StoreAsync();
                    await writer.FlushAsync();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
