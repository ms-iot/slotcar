using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Devices.Gpio;

namespace Slotcar
{
    class TrackSensors
    {
        public const int StartLineLane1_GPIO = 16;
        public const int Turn1EnterLane1_GPIO = 26;
        public const int Turn1ExitLane1_GPIO = 25;
        public const int Turn2EnterLane1_GPIO = 24;
        public const int Turn2ExitLane1_GPIO = 23;

        public const int StartLineLane2_GPIO = 12;
        public const int Turn1EnterLane2_GPIO = 13;
        public const int Turn1ExitLane2_GPIO = 5;
        public const int Turn2EnterLane2_GPIO = 27;
        public const int Turn2ExitLane2_GPIO = 22;

        public const int StartButton_GPIO = 0;


        public GpioPin StartLineLane1;
        public GpioPin Turn1EnterLane1;
        public GpioPin Turn1ExitLane1;
        public GpioPin Turn2EnterLane1;
        public GpioPin Turn2ExitLane1;

        public GpioPin StartLineLane2;
        public GpioPin Turn1EnterLane2;
        public GpioPin Turn1ExitLane2;
        public GpioPin Turn2EnterLane2;
        public GpioPin Turn2ExitLane2;

        public GpioPin StartButton;

        private void SetupSensor(out GpioPin pin, int pinNumber, GpioPinDriveMode mode = GpioPinDriveMode.Input)
        {
            pin = null;
            try
            {
                Debug.WriteLine("SetupSensor: " + pinNumber);
                pin = gpio.OpenPin(pinNumber);

                pin.SetDriveMode(mode);

                pin.ValueChanged += (gpioPin, eventArgs) =>
                {
                    Debug.WriteLine("ValueChanged: " + gpioPin.PinNumber);
                    ReadSensor(gpioPin, eventArgs);
                };
            }
            catch (Exception e)
            {
                // If you hit this your probably trying to use a reserved pin.
                Debug.WriteLine("Exception {0} when opening pin: {1}", e.Message, pinNumber);
            }

        }

        GpioController gpio;
        public void SetupSensors()
        {
            gpio = GpioController.GetDefault();
            SetupSensor(out StartLineLane1, StartLineLane1_GPIO);
            SetupSensor(out Turn1EnterLane1, Turn1EnterLane1_GPIO);
            SetupSensor(out Turn1ExitLane1, Turn1ExitLane1_GPIO);
            SetupSensor(out Turn2EnterLane1, Turn2EnterLane1_GPIO);
            SetupSensor(out Turn2ExitLane1, Turn2ExitLane1_GPIO);

            SetupSensor(out StartLineLane2, StartLineLane2_GPIO);
            SetupSensor(out Turn1EnterLane2, Turn1EnterLane2_GPIO);
            SetupSensor(out Turn1ExitLane2, Turn1ExitLane2_GPIO);
            SetupSensor(out Turn2EnterLane2, Turn2EnterLane2_GPIO);
            SetupSensor(out Turn2ExitLane2, Turn2ExitLane2_GPIO);

            SetupSensor(out StartButton, StartButton_GPIO);

        }

        public void Dispose()
        {
            StartLineLane1.Dispose();
            Turn1EnterLane1.Dispose();
            Turn1ExitLane1.Dispose();
            Turn2EnterLane1.Dispose();
            Turn2ExitLane1.Dispose();

            StartLineLane2.Dispose();
            Turn1EnterLane2.Dispose();
            Turn1ExitLane2.Dispose();
            Turn2EnterLane2.Dispose();
            Turn2ExitLane2.Dispose();

            StartButton.Dispose();

        }

        private Dictionary<int, DateTime> DebounceTimes = new Dictionary<int, DateTime>();
        private void ReadSensor(GpioPin whichPin, GpioPinValueChangedEventArgs eventArgs)
        {
            //Debug.WriteLine("ReadSensor: " + whichPin.PinNumber);
            GpioPinValue pinValue = (eventArgs.Edge == GpioPinEdge.RisingEdge) ? GpioPinValue.High : GpioPinValue.Low;
            bool skip = false;
            //DateTime lastRead;
            //if ((eventArgs.Edge == GpioPinEdge.RisingEdge) &&  DebounceTimes.TryGetValue(whichPin.PinNumber, out lastRead))
            //{
            //    TimeSpan time = DateTime.Now - lastRead;
            //    if (time.TotalMilliseconds < 10)
            //    {
            //        skip = true;
            //    }
            //}

            //DebounceTimes[whichPin.PinNumber] = DateTime.Now;

            // We will update the ux on all events, we don't care if the display flickers a bit
            MainPage.OnuxUpdate(whichPin.PinNumber, pinValue);

            if (!skip)
            {
                // Skip processing if the last interupt for this pin was < 100 milliseconds ago.
                switch (whichPin.PinNumber)
                {
                    case StartButton_GPIO:
                        {
                            Debug.WriteLine("Start Button");
                            // Can only start the game if we are in the waiting state.
                            if (RaceController.theRaceController.State == RaceState.Waiting)
                            {
                                RaceController.theRaceController.State = RaceState.Starting;
                            }
                        }
                        break;

                    case StartLineLane1_GPIO:
                        {
                            Debug.WriteLine("Start Line Lane 1");
                            UpdateLap(eventArgs, pinValue, Player.Lane1);
                        }
                        break;

                    case StartLineLane2_GPIO:
                        {
                            Debug.WriteLine("Start Line Lane 2");
                            UpdateLap(eventArgs, pinValue, Player.Lane2);
                        }
                        break;

                    case Turn1EnterLane1_GPIO:
                        {
                            Debug.WriteLine("Turn 1 Enter Lane 1");
                        }
                        break;

                    case Turn1EnterLane2_GPIO:
                        {
                            Debug.WriteLine("Turn 1 Enter Lane 2");
                        }
                        break;

                    case Turn1ExitLane1_GPIO:
                        {
                            Debug.WriteLine("Turn 1 Exit Lane 1");
                        }
                        break;

                    case Turn1ExitLane2_GPIO:
                        {
                            Debug.WriteLine("Turn 1 Exit Lane 2");
                        }
                        break;


                    case Turn2EnterLane1_GPIO:
                        {
                            Debug.WriteLine("Turn 2 Enter Lane 1");
                        }
                        break;

                    case Turn2EnterLane2_GPIO:
                        {
                            Debug.WriteLine("Turn 2 Enter Lane 2");
                        }
                        break;

                    case Turn2ExitLane1_GPIO:
                        {
                            Debug.WriteLine("Turn 2 Exit Lane 1");
                        }
                        break;

                    case Turn2ExitLane2_GPIO:
                        {
                            Debug.WriteLine("Turn 2 Exit Lane 2");
                        }
                        break;
                }
            }

        }

        private void UpdateLap(GpioPinValueChangedEventArgs eventArgs, GpioPinValue pinValue, Player whichPlayer)
        {
            if (eventArgs.Edge == GpioPinEdge.FallingEdge)
            {
                RaceController.theRaceController.Update(whichPlayer);
            }
        }

    }
}
