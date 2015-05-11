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
using Windows.Devices.Gpio;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Slotcar
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

        public Timer timer;
        public MainPage()
        {
            InitializeComponent();

            uxUpdate += MainPage_uxUpdate;

            Unloaded += MainPage_Unloaded;

            InitGPIO();

            InitUx();

            timer = new Timer(timerCallback, this, 0, 100);
        }

        private async void timerCallback(object state)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
            () =>
            {
                _BestTimeLane1.Text = RaceController.theRaceController.BestTimeLane1;
                _BestTimeLane2.Text = RaceController.theRaceController.BestTimeLane2;

                for (int i = 0; i < RaceController.NumberOfLaps; i++)
                {
                    TextBlock lane1Lap = _Lane1Laps.Children[i] as TextBlock;
                    TextBlock lane2Lap = _Lane2Laps.Children[i] as TextBlock;

                    lane1Lap.Text = RaceController.theRaceController.LapTime(Player.Lane1, i);
                    lane2Lap.Text = RaceController.theRaceController.LapTime(Player.Lane2, i);
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

                uxElement.Fill = (args.PinValue == GpioPinValue.High) ? redBrush : grayBrush;

                _BestTimeLane1.Text = RaceController.theRaceController.BestTimeLane1;
                _BestTimeLane2.Text = RaceController.theRaceController.BestTimeLane2;

                for (int i = 0; i < RaceController.NumberOfLaps; i++)
                {
                    TextBlock lane1Lap = _Lane1Laps.Children[i] as TextBlock;
                    TextBlock lane2Lap = _Lane2Laps.Children[i] as TextBlock;

                    lane1Lap.Text = RaceController.theRaceController.LapTime(Player.Lane1, i);
                    lane2Lap.Text = RaceController.theRaceController.LapTime(Player.Lane2, i);
                }


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
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

            trackSensors = new TrackSensors();
            trackSensors.SetupSensors();

            GpioStatus.Text = "GPIO initialized.";
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            // Cleanup
            trackSensors.Dispose();
        }

        void InitUx()
        {
            GPIOPinToUxBindingDictionary[trackSensors.StartButton.PinNumber] = new UXBinding(trackSensors.StartButton, _StartButton);

            GPIOPinToUxBindingDictionary[trackSensors.StartLineLane1.PinNumber] = new UXBinding(trackSensors.StartLineLane1, _StartLineLane1);
            GPIOPinToUxBindingDictionary[trackSensors.StartLineLane2.PinNumber] = new UXBinding(trackSensors.StartLineLane2, _StartLineLane2);

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

                header.Text = "Lap " + (i+1);
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


        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
    }
}
