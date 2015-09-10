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
        HostName serverHost;

        const string gamePort1 = "25666";
        const string gamePort2 = "25667";

        const long TrackUpdateInterval = 100;   // Update Interval in Milliseconds
        bool processing = false;

        private TrackLane connectedToLane;
        DateTime lastUpdated;

        private string portToUse()
        {
            string port = gamePort2;
            if (connectedToLane == TrackLane.Lane1)
            {
                port = gamePort1;
            }

            return port;

        }

        public TrackControl()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {           
            lastUpdated = DateTime.Now;
            var init = e.Parameter as TrackControlInit;
            TrackDisplay.Text = "Track " + ((int)init.lane + 1);
            if (init != null && !string.IsNullOrEmpty(init.computerName))
            {
                connectedToLane = init.lane;
                serverHost = new HostName(init.computerName);
            }
            Accelerometer accel = Accelerometer.GetDefault();
            accel.ReadingChanged += Accelerometer_ReadingChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Accelerometer.GetDefault().ReadingChanged -= Accelerometer_ReadingChanged;
        }

        private async void Accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        {
            if (DateTime.Now.Subtract(lastUpdated).Milliseconds < TrackUpdateInterval || processing)
            {
                return;
            }

            lastUpdated = DateTime.Now;
            processing = true;

            double speed = (1.0 - args.Reading.AccelerationX);
            var i = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // TODO: Add visualization

               // Speed.Value =  speed * Speed.Maximum;
            });

            // Scale number to 0 - .75;
            double pwm = (speed * 0.75) * 255.0;
            try
            {
                string s = "{\"PWM\": \"" + pwm.ToString("0") + "\"}\n";

                using (StreamSocket clientSocket = new StreamSocket())
                {
                    DataWriter writer = null;
                    await clientSocket.ConnectAsync(serverHost, portToUse());
                    using (writer = new DataWriter(clientSocket.OutputStream))
                    {

                        uint len = writer.WriteString(s);
                        await writer.StoreAsync();
                        await writer.FlushAsync();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            processing = false;
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
