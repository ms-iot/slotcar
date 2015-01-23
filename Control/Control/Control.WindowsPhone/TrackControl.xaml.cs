using Control.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
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

namespace Control
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TrackControl : Page
    {
        StreamSocket clientSocket = new StreamSocket();
        DataWriter writer = null;
        bool connected = false;

        HostName serverHost = new HostName("lamadio_dec");

        const string gamePort1 = "25666";
        const string gamePort2 = "25667";

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public TrackControl()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            progress.IsActive = true;
            Accelerometer.GetDefault().ReadingChanged += Accelerometer_ReadingChanged;

            if (e.Parameter is TrackLane)
            {
                string port = gamePort2;
                if ((TrackLane)(e.Parameter)== TrackLane.Lane1)
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
            this.navigationHelper.OnNavigatedFrom(e);
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

        #endregion

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

                Speed.Value =  speed * Speed.Maximum;
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
