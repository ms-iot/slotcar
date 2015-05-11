using System;
using System.Diagnostics;

namespace Slotcar
{
    public enum RaceState { Waiting, Starting, Running, Over };
    public enum Player { Lane1, Lane2 };

    class RaceController
    {
        public static RaceController theRaceController = new RaceController();
        public const int NumberOfLaps = 5;

        private RaceState raceState = RaceState.Waiting;

        LapTimeController[] LapControllers;

        public RaceState State
        {
            get { return raceState; }
            set
            {
                raceState = value;
                switch (raceState)
                {
                    case RaceState.Waiting:
                        {

                        }
                        break;

                    case RaceState.Starting:
                        {
                            Start();
                        }
                        break;

                    case RaceState.Running:
                        {
                            Running();
                        }
                        break;

                    case RaceState.Over:
                        {
                            End();
                        }
                        break;
                }
            }
        }

        public string BestTimeLane1 { get
            {
                if (State == RaceState.Running)
                {
                    return LapControllers[0].BestTime;
                }
                else
                {
                    return "Waiting";
                }
            }
        }

        public string BestTimeLane2
        {
            get
            {
                if (State == RaceState.Running)
                {
                    return LapControllers[1].BestTime;
                }
                else
                {
                    return "Waiting";
                }
            }
        }

        void Start()
        {
            Debug.WriteLine("RaceController::Start");
            // Zero out previous results
            LapControllers = new LapTimeController[2];
            LapControllers[0] = new LapTimeController(NumberOfLaps);
            LapControllers[1] = new LapTimeController(NumberOfLaps);

            // Start countdown
            // Enable controls
            State = RaceState.Running;
        }

        void Running()
        {
            Debug.WriteLine("RaceController::Running");
            // Enable controls
        }

        void End()
        {
            Debug.WriteLine("RaceController::End");

            // Disable controls
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
                End();
            }
        }

        internal string LapTime(Player whichPlayer, int whichLap)
        {
            if (State == RaceState.Running)
            {
                return LapControllers[(int)whichPlayer].LapTime(whichLap);
            }
            else
            {
                return "Waiting";
            }
        }
    }
}
