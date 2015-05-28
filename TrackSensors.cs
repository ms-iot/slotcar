using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Devices.Gpio;

namespace SlotCar
{

    public class TrackSensors
    {
        // Pi2 Available GPIO lines
        //GPIO_27
        //GPIO_22
        //GPIO_0
        //GPIO_5
        //GPIO_6
        //GPIO_13
        //GPIO_26
        //GPIO_18
        //GPIO_23
        //GPIO_24
        //GPIO_25
        //GPIO_1
        //GPIO_12
        //GPIO_16

        public const int StartLineLane1_GPIO = 13;
        public const int ReadyLineLane1_GPIO = 6;
        public const int Turn1EnterLane1_GPIO = 16;
        public const int Turn1ExitLane1_GPIO = 24;
        public const int Turn2EnterLane1_GPIO = 18;
        public const int Turn2ExitLane1_GPIO = 27;

        public const int StartLineLane2_GPIO = 26;
        public const int ReadyLineLane2_GPIO = 5;
        public const int Turn1EnterLane2_GPIO = 12;
        public const int Turn1ExitLane2_GPIO = 25;
        public const int Turn2EnterLane2_GPIO = 23;
        public const int Turn2ExitLane2_GPIO = 22;


        public GpioPin StartLineLane1;
        public GpioPin ReadyLineLane1;
        public GpioPin Turn1EnterLane1;
        public GpioPin Turn1ExitLane1;
        public GpioPin Turn2EnterLane1;
        public GpioPin Turn2ExitLane1;

        public GpioPin StartLineLane2;
        public GpioPin ReadyLineLane2;
        public GpioPin Turn1EnterLane2;
        public GpioPin Turn1ExitLane2;
        public GpioPin Turn2EnterLane2;
        public GpioPin Turn2ExitLane2;

        public GpioPin StartButton1;
        public GpioPin StartButton2;

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
                    GPIOHandler(gpioPin, eventArgs);
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
            SetupSensor(out ReadyLineLane1, ReadyLineLane1_GPIO);
            SetupSensor(out Turn1EnterLane1, Turn1EnterLane1_GPIO);
            SetupSensor(out Turn1ExitLane1, Turn1ExitLane1_GPIO);
            SetupSensor(out Turn2EnterLane1, Turn2EnterLane1_GPIO);
            SetupSensor(out Turn2ExitLane1, Turn2ExitLane1_GPIO);

            SetupSensor(out StartLineLane2, StartLineLane2_GPIO);
            SetupSensor(out ReadyLineLane2, ReadyLineLane2_GPIO);
            SetupSensor(out Turn1EnterLane2, Turn1EnterLane2_GPIO);
            SetupSensor(out Turn1ExitLane2, Turn1ExitLane2_GPIO);
            SetupSensor(out Turn2EnterLane2, Turn2EnterLane2_GPIO);
            SetupSensor(out Turn2ExitLane2, Turn2ExitLane2_GPIO);
        }

        public void Dispose()
        {
            StartLineLane1.Dispose();
            ReadyLineLane1.Dispose();
            Turn1EnterLane1.Dispose();
            Turn1ExitLane1.Dispose();
            Turn2EnterLane1.Dispose();
            Turn2ExitLane1.Dispose();

            StartLineLane2.Dispose();
            ReadyLineLane2.Dispose();
            Turn1EnterLane2.Dispose();
            Turn1ExitLane2.Dispose();
            Turn2EnterLane2.Dispose();
            Turn2ExitLane2.Dispose();

            StartButton1.Dispose();
            StartButton2.Dispose();

        }

        private Dictionary<int, DateTime> DebounceTimes = new Dictionary<int, DateTime>();
        private void GPIOHandler(GpioPin whichPin, GpioPinValueChangedEventArgs eventArgs)
        {
            Debug.WriteLine("GPIOHandler: " + whichPin.PinNumber);
            GpioPinValue pinValue = (eventArgs.Edge == GpioPinEdge.RisingEdge) ? GpioPinValue.High : GpioPinValue.Low;
            bool skip = false;
            DateTime lastRead;
            // Any event that is less than 10ms is probably a bounce and we will ignore it.
            if (DebounceTimes.TryGetValue(whichPin.PinNumber, out lastRead))
            {
                TimeSpan time = DateTime.Now - lastRead;
                if (time.TotalMilliseconds < 10)
                {
                    skip = true;
                }
            }

            DebounceTimes[whichPin.PinNumber] = DateTime.Now;


            if (!skip)
            {
                MainPage.OnuxUpdate(whichPin.PinNumber, pinValue);
                switch (whichPin.PinNumber)
                {
                    case StartLineLane1_GPIO:
                        {
                            Debug.WriteLine("Start Line Lane 1");
                            UpdateLap(eventArgs, pinValue, Player.Lane1);
                            Globals.theRaceController.UpdateCarPosition(Player.Lane2, CarPosition.Start);
                        }
                        break;

                    case StartLineLane2_GPIO:
                        {
                            Debug.WriteLine("Start Line Lane 2");
                            UpdateLap(eventArgs, pinValue, Player.Lane2);
                            Globals.theRaceController.UpdateCarPosition(Player.Lane2, CarPosition.Start);
                        }
                        break;

                    case ReadyLineLane1_GPIO:
                        {
                            Debug.WriteLine("Ready Line Lane 1");

                            UpdateLap(eventArgs, pinValue, Player.Lane1);
                            Globals.theRaceController.UpdateCarPosition(Player.Lane1, CarPosition.Start);
                        }
                        break;

                    case ReadyLineLane2_GPIO:
                        {
                            Debug.WriteLine("Ready Line Lane 2");

                            UpdateLap(eventArgs, pinValue, Player.Lane2);
                            Globals.theRaceController.UpdateCarPosition(Player.Lane2, CarPosition.Start);
                        }
                        break;

                    case Turn1EnterLane1_GPIO:
                        {
                            Debug.WriteLine("Turn 1 Enter Lane 1");
                            Globals.theRaceController.UpdateCarPosition(Player.Lane1, CarPosition.Turn1);
                        }
                        break;

                    case Turn1EnterLane2_GPIO:
                        {
                            Debug.WriteLine("Turn 1 Enter Lane 2");
                            Globals.theRaceController.UpdateCarPosition(Player.Lane2, CarPosition.Turn1);
                        }
                        break;

                    case Turn1ExitLane1_GPIO:
                        {
                            Debug.WriteLine("Turn 1 Exit Lane 1");
                            Globals.theRaceController.UpdateCarPosition(Player.Lane1, CarPosition.Straight1);
                        }
                        break;

                    case Turn1ExitLane2_GPIO:
                        {
                            Debug.WriteLine("Turn 1 Exit Lane 2");
                            Globals.theRaceController.UpdateCarPosition(Player.Lane2, CarPosition.Straight1);
                        }
                        break;


                    case Turn2EnterLane1_GPIO:
                        {
                            Debug.WriteLine("Turn 2 Enter Lane 1");
                            Globals.theRaceController.UpdateCarPosition(Player.Lane1, CarPosition.Turn2);
                        }
                        break;

                    case Turn2EnterLane2_GPIO:
                        {
                            Debug.WriteLine("Turn 2 Enter Lane 2");
                            Globals.theRaceController.UpdateCarPosition(Player.Lane2, CarPosition.Turn2);
                        }
                        break;

                    case Turn2ExitLane1_GPIO:
                        {
                            Debug.WriteLine("Turn 2 Exit Lane 1");
                            Globals.theRaceController.UpdateCarPosition(Player.Lane1, CarPosition.Straight2);
                        }
                        break;

                    case Turn2ExitLane2_GPIO:
                        {
                            Debug.WriteLine("Turn 2 Exit Lane 2");
                            Globals.theRaceController.UpdateCarPosition(Player.Lane2, CarPosition.Straight2);
                        }
                        break;
                }
            }

        }

        private void UpdateLap(GpioPinValueChangedEventArgs eventArgs, GpioPinValue pinValue, Player whichPlayer)
        {
            if (eventArgs.Edge == GpioPinEdge.FallingEdge)
            {
                Globals.theRaceController.Update(whichPlayer);
            }
        }

    }
}
