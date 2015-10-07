/*
    Copyright(c) Microsoft Open Technologies, Inc. All rights reserved.

    The MIT License(MIT)

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files(the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions :

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Linq;
using Windows.Devices.Gpio;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml;
using Windows.Networking.Connectivity;

namespace SlotCar
{
    public delegate void uxUpdateHandler(uxUpdateEventArgs args);

    public class uxUpdateEventArgs : EventArgs
    {
        public readonly int PinNumber;
        public readonly GpioPinValue PinValue;

        public uxUpdateEventArgs(int pinNumber, GpioPinValue pinValue)
        {
            PinNumber = pinNumber;
            PinValue = pinValue;
        }

    }

    public class UXBinding
    {
        public UXBinding(GpioPin gpioPin, object uxObject)
        {
            GpioPin = gpioPin;
            UxObject = uxObject;
        }
        public readonly GpioPin GpioPin;
        public readonly object UxObject;
    }

    public sealed partial class MainPage : Page
    {
        public const int LapFontSize = 30;
        private Dictionary<int, UXBinding> GPIOPinToUxBindingDictionary = new Dictionary<int, UXBinding>();
        public static event uxUpdateHandler uxUpdate;

        public static void OnuxUpdate(int pinNumber, GpioPinValue pinValue)
        {
            if (uxUpdate != null)
            {
                uxUpdateEventArgs args = new uxUpdateEventArgs(pinNumber, pinValue);
                uxUpdate(args);
            }
        }

        const string gamePort1 = "25666";
        const string gamePort2 = "25667";

        public MotorController motorController = new MotorController();
        CommTCP track1NetworkInterface;
        CommTCP track2NetworkInterface;


        public Timer timer;
        public MainPage()
        {
            Globals.theMainPage = this;
            InitializeComponent();
            uxUpdate += MainPage_uxUpdate;

            Unloaded += MainPage_Unloaded;

        }

        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();
            if (icp != null && icp.NetworkAdapter != null)
            {
                var hostname = NetworkInformation.GetHostNames().FirstOrDefault(
                                hn =>
                                hn.IPInformation != null && hn.IPInformation.NetworkAdapter != null
                                && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                                == icp.NetworkAdapter.NetworkAdapterId && hn.Type == Windows.Networking.HostNameType.Ipv4);

                if (null != hostname)
                {
                    _IpTextBlock.Text = hostname.CanonicalName.ToString();
                }
            }

            try
            {
                await motorController.initialize();
                motorController.setSpeedA(0.0f);
                motorController.setSpeedB(0.0f);
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            try
            {
                track1NetworkInterface = new CommTCP(gamePort1);
                await track1NetworkInterface.StartServer(_IpTextBlock.Text);

                track1NetworkInterface.speedUpdate += (speed) =>
                {
                    if (Globals.theRaceController.State == RaceState.Running && Globals.theRaceController.NumberOfAutoPlayers < 2)
                    {
                        motorController.setSpeedA(speed * 0.25f / 255);
                    }
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
                await track2NetworkInterface.StartServer(_IpTextBlock.Text);
                track2NetworkInterface.speedUpdate += (speed) =>
                {
                    if (Globals.theRaceController.State == RaceState.Running && Globals.theRaceController.NumberOfAutoPlayers == 0)
                    {
                        motorController.setSpeedB(speed * 0.25f / 255);
                    }
                };
            }
            catch (Exception e)
            {
                // Server can force shutdown which generates an exception. Spew it.
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }

            InitGPIO();
            InitUx();
            timer = new Timer(timerCallback, this, 0, 100);
        }

        private async void timerCallback(object state)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
            () =>
            {
                _BestTimeLane1.Text = Globals.theRaceController.BestTimeLane1;
                _BestTimeLane2.Text = Globals.theRaceController.BestTimeLane2;

                for (int i = 0; i < RaceController.NumberOfLaps; i++)
                {
                    TextBlock lane1Lap = _Lane1Laps.Children[i] as TextBlock;
                    TextBlock lane2Lap = _Lane2Laps.Children[i] as TextBlock;

                    lane1Lap.Text = Globals.theRaceController.LapTime(Player.Lane1, i);
                    lane2Lap.Text = Globals.theRaceController.LapTime(Player.Lane2, i);
                }
            });
        }

        private async void MainPage_uxUpdate(uxUpdateEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
            () =>
            {
                var uxBinding = GPIOPinToUxBindingDictionary[args.PinNumber];
                var pin = uxBinding.GpioPin;
                var uxElement = uxBinding.UxObject as Ellipse;

                var pinValue = uxBinding.GpioPin.Read();
                uxElement.Fill = (pinValue == GpioPinValue.High) ? redBrush : grayBrush;

                _BestTimeLane1.Text = Globals.theRaceController.BestTimeLane1;
                _BestTimeLane2.Text = Globals.theRaceController.BestTimeLane2;

                for (int i = 0; i < RaceController.NumberOfLaps; i++)
                {
                    TextBlock lane1Lap = _Lane1Laps.Children[i] as TextBlock;
                    TextBlock lane2Lap = _Lane2Laps.Children[i] as TextBlock;

                    lane1Lap.Text = Globals.theRaceController.LapTime(Player.Lane1, i);
                    lane2Lap.Text = Globals.theRaceController.LapTime(Player.Lane2, i);
                }

                _BestLapOfTheDay.Text = Globals.theRaceController.BestTimeAsString;

            });
        }

        GpioController gpio;
        TrackSensors trackSensors;

        private void InitGPIO()
        {
            Debug.WriteLine("InitGPIO");
            gpio = GpioController.GetDefault();
            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                _Status.Text = "There is no GPIO controller on this device.";
                return;
            }

            trackSensors = Globals.theTrackSensors;
            trackSensors.SetupSensors();

            _Status.Text = "GPIO initialized.";
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            // Cleanup
            trackSensors.Dispose();
        }

        void InitUx()
        {
            GPIOPinToUxBindingDictionary[trackSensors.StartLineLane1.PinNumber] = new UXBinding(trackSensors.StartLineLane1, _StartLineLane1);
            GPIOPinToUxBindingDictionary[trackSensors.StartLineLane2.PinNumber] = new UXBinding(trackSensors.StartLineLane2, _StartLineLane2);

            GPIOPinToUxBindingDictionary[trackSensors.ReadyLineLane1.PinNumber] = new UXBinding(trackSensors.ReadyLineLane1, _ReadyLineLane1);
            GPIOPinToUxBindingDictionary[trackSensors.ReadyLineLane2.PinNumber] = new UXBinding(trackSensors.ReadyLineLane2, _ReadyLineLane2);

            GPIOPinToUxBindingDictionary[trackSensors.Turn1EnterLane1.PinNumber] = new UXBinding(trackSensors.Turn1EnterLane1, _Turn1EnterLane1);
            GPIOPinToUxBindingDictionary[trackSensors.Turn1EnterLane2.PinNumber] = new UXBinding(trackSensors.Turn1EnterLane2, _Turn1EnterLane2);
            GPIOPinToUxBindingDictionary[trackSensors.Turn1ExitLane1.PinNumber] = new UXBinding(trackSensors.Turn1ExitLane1, _Turn1ExitLane1);
            GPIOPinToUxBindingDictionary[trackSensors.Turn1ExitLane2.PinNumber] = new UXBinding(trackSensors.Turn1ExitLane2, _Turn1ExitLane2);

            GPIOPinToUxBindingDictionary[trackSensors.Turn2EnterLane1.PinNumber] = new UXBinding(trackSensors.Turn2EnterLane1, _Turn2EnterLane1);
            GPIOPinToUxBindingDictionary[trackSensors.Turn2EnterLane2.PinNumber] = new UXBinding(trackSensors.Turn2EnterLane2, _Turn2EnterLane2);
            GPIOPinToUxBindingDictionary[trackSensors.Turn2ExitLane1.PinNumber] = new UXBinding(trackSensors.Turn2ExitLane1, _Turn2ExitLane1);
            GPIOPinToUxBindingDictionary[trackSensors.Turn2ExitLane2.PinNumber] = new UXBinding(trackSensors.Turn2ExitLane2, _Turn2ExitLane2);

            foreach (var entry in GPIOPinToUxBindingDictionary.Values)
            {
                var pin = entry.GpioPin;
                var uxElement = entry.UxObject as Ellipse;

                var pinValue = pin.Read();
                uxElement.Fill = (pinValue == GpioPinValue.High) ? redBrush : grayBrush;
                var pinElement = this.FindName(uxElement.Name + "Pin") as TextBlock;
                if (pinElement != null)
                {
                    pinElement.Text = pin.PinNumber.ToString();
                }
            }

            _BestTimeLane1.Text = "00:00.000";
            _BestTimeLane2.Text = "00:00.000";

            _LapHeaders.Children.Clear();
            _Lane1Laps.Children.Clear();
            _Lane2Laps.Children.Clear();

            for (int i = 0; i < RaceController.NumberOfLaps; i++)
            {
                TextBlock header = new TextBlock();
                TextBlock turn1 = new TextBlock();
                TextBlock turn2 = new TextBlock();

                header.Text = "Lap " + (i + 1);
                header.FontSize = LapFontSize;

                turn1.Text = "00:00.000" + i;
                turn1.FontSize = LapFontSize;

                turn2.Text = "00:00.00" + i + "0";
                turn2.FontSize = LapFontSize;

                _LapHeaders.Children.Add(header);
                _Lane1Laps.Children.Add(turn1);
                _Lane2Laps.Children.Add(turn2);
            }

        }

        public async void ShowWinner(Player whichPlayer)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
            () =>
            {
                switch (whichPlayer)
                {
                    case Player.Lane1:
                        {
                            _Winner1.Visibility = Visibility.Visible;
                        }
                        break;

                    case Player.Lane2:
                        {
                            _Winner2.Visibility = Visibility.Visible;
                        }
                        break;
                }
            });
        }

        public async void ClearWinner()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
            () =>
            {
                _Winner1.Visibility = Visibility.Collapsed;
                _Winner2.Visibility = Visibility.Collapsed;
            });
        }

        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);

        private void OnResetButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Reset Button");
            Globals.theRaceController.ResetRace();

        }
        private void OnAutoButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Auto Button");
            Globals.theRaceController.StartRace(2, float.Parse(_MaxSpeed1Textbox.Text), float.Parse(_MaxSpeed2Textbox.Text));
        }
        private void On1PlayerButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("1 Player Button");
            Globals.theRaceController.StartRace(1, .0f, float.Parse(_MaxSpeed2Textbox.Text));
        }
        private void On2PlayerButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("2 Player Button");
            Globals.theRaceController.StartRace(0, .0f, .0f);
        }
    }
}
