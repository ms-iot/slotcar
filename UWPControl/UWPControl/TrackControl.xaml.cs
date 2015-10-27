using System;
using System.Diagnostics;
using Windows.Devices.Sensors;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
        private const string gamePort1 = "25666";
        private const string gamePort2 = "25667";
        private const long trackUpdateIntervalMs = 100;

        private Accelerometer accelerometer = Accelerometer.GetDefault();
        private DatagramSocket udpSocket = new DatagramSocket();
        private DataWriter udpPipe;
        private ThreadPoolTimer readTimer;

        public TrackControl()
        {
            InitializeComponent();
        }

        async protected override void OnNavigatedTo(NavigationEventArgs e)
        {           
            var init = e.Parameter as TrackControlInit;
            TrackDisplay.Text = "Track " + ((int)init.lane + 1);
            if (init != null && !string.IsNullOrEmpty(init.computerName))
            {
                HostName serverHost = new HostName(init.computerName);
                string gamePort;
                if ( TrackLane.Lane1 == init.lane )
                {
                    gamePort = gamePort1;
                }
                else
                {
                    gamePort = gamePort2;
                }
                await udpSocket.ConnectAsync(serverHost, gamePort);
                var outputStream = await udpSocket.GetOutputStreamAsync(serverHost, gamePort);
                udpPipe = new DataWriter(outputStream);
            }
            readTimer = ThreadPoolTimer.CreatePeriodicTimer(ReadAccelerometer, TimeSpan.FromMilliseconds(trackUpdateIntervalMs));
    }

        async protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            await udpSocket.CancelIOAsync();
            udpPipe.Dispose();
            readTimer.Cancel();
        }

        private async void ReadAccelerometer(ThreadPoolTimer timer)
        {
            readTimer.Cancel();
            double speed = (1.0 - accelerometer.GetCurrentReading().AccelerationX);
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

                udpPipe.WriteString(s);
                await udpPipe.StoreAsync();
                await udpPipe.FlushAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            readTimer = ThreadPoolTimer.CreatePeriodicTimer(ReadAccelerometer, TimeSpan.FromMilliseconds(trackUpdateIntervalMs));
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
