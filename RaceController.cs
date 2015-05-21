using System;
using System.Diagnostics;

namespace SlotCar
{
    public class RaceController
    {
        public const int NumberOfLaps = 5;

        private int NumberOfPlayers = 2;
        private RaceState raceState = RaceState.Waiting;

        LapTimeController[] LapControllers = new LapTimeController[2];
        private Stig ComputerPlayer = null;

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
                                Start(NumberOfPlayers);
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

                    case RaceState.Resetting:
                        {
                            // Move cars around the track to reset starting positions.
                            raceState = value;

                            bool Player1Ready = Reset(Player.Lane1);
                            bool Player2Ready = Reset(Player.Lane2);
                            if (Player1Ready && Player2Ready)
                            {
                                State = RaceState.Waiting;
                            }
                        }
                        break;
                }
            }
        }

        private void ShowWinner()
        {
            Globals.theMainPage.ShowWinner(Player.Lane1);
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

        private bool Reset(Player whichPlayer)
        {
            bool playerReadyForNextRace = false;

            switch (whichPlayer)
            {
                case Player.Lane1:
                    {
                        if (Globals.theTrackSensors.IsPinSet(TrackSensors.ReadyLineLane1_GPIO))
                        {
                            Globals.theMainPage.motorController.setSpeedA(0);
                            playerReadyForNextRace = true;
                        }
                    }
                    break;
                case Player.Lane2:
                    {
                        if (Globals.theTrackSensors.IsPinSet(TrackSensors.ReadyLineLane2_GPIO))
                        {
                            Globals.theMainPage.motorController.setSpeedB(0);
                            playerReadyForNextRace = true;
                        }
                    }
                    break;
            }

            return playerReadyForNextRace;
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


        void Start(int numberOfPlayers)
        {
            Debug.WriteLine("RaceController::Start");

            Globals.theMainPage.ClearWinner();

            // Zero out previous results
            LapControllers = new LapTimeController[2];
            LapControllers[0] = new LapTimeController(NumberOfLaps);
            LapControllers[1] = new LapTimeController(NumberOfLaps);

            NumberOfPlayers = numberOfPlayers;
            if (NumberOfPlayers == 1)
            {
                // Need a computer
                ComputerPlayer = new Stig(Player.Lane1);
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
                Globals.theMainPage.motorController.setSpeedA(.25f);
            }

            Debug.WriteLine("Lane2 Motor on");
            Globals.theMainPage.motorController.setSpeedB(.25f);

        }

        void End()
        {
            Debug.WriteLine("RaceController::End");

            // Disable controls
            Debug.WriteLine("Lane1 Motor off");
            Globals.theMainPage.motorController.setSpeedA(.0f);

            Debug.WriteLine("Lane2 Motor off");
            Globals.theMainPage.motorController.setSpeedB(.0f);

            // done with the computer
            ComputerPlayer = null;

            // Display winner
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

        internal void StartRace(int numberOfPlayers)
        {
            Debug.WriteLine("StartRace: {0}", numberOfPlayers);
            // only allow starting a new race if we are currently waiting for a start.
            if (raceState == RaceState.Waiting)
            {
                NumberOfPlayers = numberOfPlayers;
                State = RaceState.Starting;
            }
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
