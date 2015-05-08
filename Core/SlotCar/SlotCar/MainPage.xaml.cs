using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SlotCar
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const string gamePort1 = "25666";
        const string gamePort2 = "25667";

        MotorController motorController = new MotorController();
        CommTCP track1NetworkInterface;
        CommTCP track2NetworkInterface;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            await motorController.initialize();
            motorController.setSpeedA(0.0f);
            motorController.setSpeedB(0.0f);


            try
            {
                track1NetworkInterface = new CommTCP(gamePort1);
                await track1NetworkInterface.StartServer();

                track1NetworkInterface.speedUpdate += (speed) =>
                {
                    motorController.setSpeedA(speed);
                };
            }
            catch (Exception e)
            {
                // Server can force shutdown which generates an exception. Spew it.
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }

            try
            {
                track2NetworkInterface = new CommTCP(gamePort2);
                await track2NetworkInterface.StartServer();
                track2NetworkInterface.speedUpdate += (speed) =>
                {
                // negative because of how the track is wired
                motorController.setSpeedB(-speed);
                };
            }
            catch (Exception e)
            {
                // Server can force shutdown which generates an exception. Spew it.
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }

        }
    }
}
