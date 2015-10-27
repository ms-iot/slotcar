using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPControl
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        string networkName;
        public MainPage()
        {
            this.InitializeComponent();
        }
        private void Track1_Click(object sender, RoutedEventArgs e)
        {
            if (networkName != NetworkName.Text)
            {
                networkName = NetworkName.Text;
            }
            TrackControlInit init = new TrackControlInit { lane = TrackLane.Lane1, computerName = NetworkName.Text };
            Frame.Navigate(typeof(TrackControl), init);
        }

        private void Track2_Click(object sender, RoutedEventArgs e)
        {
            if (networkName != NetworkName.Text)
            {
                networkName = NetworkName.Text;
            }
            TrackControlInit init = new TrackControlInit { lane = TrackLane.Lane2, computerName = NetworkName.Text };
            Frame.Navigate(typeof(TrackControl), init);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (null != networkName) { NetworkName.Text = networkName; }
        }
    }
}
