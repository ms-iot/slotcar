using System;
using System.Diagnostics;

namespace SlotCar
{
    public class RaceController
    {
        public const int NumberOfLaps = 5;

        private int NumberOfPlayers = 2;
        private float MaxSpeed1 = .0f;
        private float MaxSpeed2 = .0f;
        private RaceState raceState = RaceState.Waiting;

        LapTimeController[] LapControllers = new LapTimeController[2];
        private AutoRacer ComputerPlayer = null;

        TimeSpan BestLapTime = TimeSpan.MaxValue;

        public string BestTimeAsString
        {
            get
            {
                if (BestLapTime == TimeSpan.MaxValue)
                {
                    return "Waiting";
                }
                else
                {
                    return String.Format("{0,2:00}:{1,2:00}.{2,3:000}", BestLapTime.Minutes, BestLapTime.Seconds, BestLapTime.Milliseconds);
                }
            }
        }



        public RaceState State
        {
            get { return raceState; }
            set
            {
                switch (value)
                {
                    case RaceState.Waiting:
                        {
                            raceState = value;
                        }
                        break;

                    case RaceState.Starting:
                        {
                            // Can only start a race if we are in the waiting state currently.
                            if (raceState == RaceState.Waiting)
                            {
                                raceState = value;
                                Start(NumberOfPlayers, MaxSpeed1, MaxSpeed2);
                            }
                        }
                        break;

                    case RaceState.Running:
                        {
                            raceState = value;
                            Running();
                        }
                        break;

                    case RaceState.Over:
                        {
                            raceState = value;
                            UpdateBestLapTimeToDate();
                            ShowWinner();
                            End();
                        }
                        break;
                }
            }
        }

        private void ShowWinner()
        {
            Globals.theMainPage.ShowWinner(Globals.theRaceController.Winner);
        }

        private void UpdateBestLapTimeToDate()
        {
            if (BestLapTime > LapControllers[1].BestTime)
            {
                BestLapTime = LapControllers[1].BestTime;
            }

            if (BestLapTime > LapControllers[1].BestTime)
            {
                BestLapTime = LapControllers[1].BestTime;
            }
        }

        internal void SetSpeed(Player whichPlayer, float newSpeed)
        {
            switch(whichPlayer)
            {
                case Player.Lane1:
                    {
                        Globals.theMainPage.motorController.setSpeedA(newSpeed);
                    }break;
                case Player.Lane2:
                    {
                        Globals.theMainPage.motorController.setSpeedB(newSpeed);
                    }
                    break;
            }
        }

        public string BestTimeLane1 {
            get
            {
                if (LapControllers[0] == null)
                {
                    return "--:--.----";
                }
                else
                {
                    return LapControllers[0].BestTimeAsString;
                }
            }
        }

        public string BestTimeLane2
        {
            get
            {
                if (LapControllers[1] == null)
                {
                    return "--:--.----";
                }
                else
                {
                    return LapControllers[1].BestTimeAsString;
                }
            }
        }

        public Player Winner { get; private set; }

        void Start(int numberOfPlayers,float maxSpeed1, float maxSpeed2)
        {
            // TODO: 
            // Verify the cars are both on the Ready line
            // Do a start countdown on screen
            // Transfer control of the cars to the phone(s) depending on number of players.

            Debug.WriteLine("RaceController::Start");

            Globals.theMainPage.ClearWinner();

            // Zero out previous results
            LapControllers = new LapTimeController[2];
            LapControllers[0] = new LapTimeController(NumberOfLaps);
            LapControllers[1] = new LapTimeController(NumberOfLaps);

            NumberOfPlayers = numberOfPlayers;
            MaxSpeed1 = maxSpeed1;
            MaxSpeed2 = maxSpeed2;

            if (NumberOfPlayers == 1)
            {
                // Need a computer
                ComputerPlayer = new AutoRacer(Player.Lane1);
            }
            // Start countdown
            // Enable controls
            State = RaceState.Running;
        }

        void Running()
        {
            Debug.WriteLine("RaceController::Running");
            // Enable controls

            if (ComputerPlayer != null)
            {
                ComputerPlayer.Go();
            }
            else
            {
                Debug.WriteLine("Lane1 Motor on");
                Globals.theMainPage.motorController.setSpeedA(MaxSpeed1);
            }

            Debug.WriteLine("Lane2 Motor on");
            Globals.theMainPage.motorController.setSpeedB(MaxSpeed2);

        }

        void End()
        {
            Debug.WriteLine("RaceController::End");

            // done with the computer
            ComputerPlayer = null;

            // Disable controls

            // Turn off motors (do this after getting rid of the computer player so if it coasts over a sensor it doesn't overide the speed.
            Debug.WriteLine("Lane1 Motor off");
            Globals.theMainPage.motorController.setSpeedA(.0f);

            Debug.WriteLine("Lane2 Motor off");
            Globals.theMainPage.motorController.setSpeedB(.0f);

            // Go back to idle
            State = RaceState.Waiting;
        }

        public void Update(Player whichPlayer)
        {
            Debug.WriteLine("Update {0}", whichPlayer);
            bool endOfRace = false;
            if (State == RaceState.Running)
            { 
                if (whichPlayer == Player.Lane1)
                {
                    endOfRace = LapControllers[0].Update(whichPlayer);
                }
                else
                {
                    endOfRace = LapControllers[1].Update(whichPlayer);
                }
            }

            if (endOfRace)
            {
                Winner = whichPlayer;
                Debug.WriteLine("End of race detected");
                State = RaceState.Over;
            }
        }

        internal string LapTime(Player whichPlayer, int whichLap)
        {
            if (LapControllers[(int)whichPlayer] == null)
            {
                return "--:--.----";
            }
            else
            {
                return LapControllers[(int)whichPlayer].LapTime(whichLap);
            }
        }

        internal void StartRace(int numberOfPlayers, float maxSpeed1, float maxSpeed2)
        {
            Debug.WriteLine("StartRace: {0} Speed1: {1} Speed2: {2}", numberOfPlayers, maxSpeed1, maxSpeed2);
            // only allow starting a new race if we are currently waiting for a start.
            if (raceState == RaceState.Waiting)
            {
                NumberOfPlayers = numberOfPlayers;
                MaxSpeed1 = maxSpeed1;
                MaxSpeed2 = maxSpeed2;
                State = RaceState.Starting;
            }
        }

        internal void ResetRace()
        {
            Debug.WriteLine("Resetting race");

            //Stop race
            State = RaceState.Over;

            // Zero out previous results
            Globals.theMainPage.ClearWinner();

            LapControllers = new LapTimeController[2];
            LapControllers[0] = new LapTimeController(NumberOfLaps);
            LapControllers[1] = new LapTimeController(NumberOfLaps);
        }
        internal void UpdateCarPosition(Player whichPlayer, CarPosition whatPosition)
        {
            if (ComputerPlayer != null)
            {
                ComputerPlayer.UpdatePosition(whatPosition);
            }
        }
    }
}
