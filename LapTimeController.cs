using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slotcar
{
    class LapTimeController
    {
        Lap[] LapsList;
        int CurrentLap = 0;
        int NumberOfLaps = 0;

        TimeSpan BestLapTime = TimeSpan.MaxValue;

        public string BestTime
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

        public string LapTime(int whichLap)
        {
            return LapsList[whichLap].LapTime;
        }

        public LapTimeController(int numberOfLaps)
        {
            NumberOfLaps = numberOfLaps;
            LapsList = new Lap[NumberOfLaps];
            for (int i = 0; i < LapsList.Length; i++)
            {
                LapsList[i] = new Lap();
            }
        }

        public bool Update(Player whichPlayer)
        {
            bool raceOver = false;
            if (LapsList[CurrentLap].Update())
            {
                if (CurrentLap < NumberOfLaps - 1)
                {
                    if (LapsList[CurrentLap].Duration < BestLapTime)
                    {
                        BestLapTime = LapsList[CurrentLap].Duration;
                    }

                    CurrentLap++;

                    Debug.WriteLine("Starting lap {0} player: " + whichPlayer, CurrentLap);

                    LapsList[CurrentLap].Update();  // Get the start time in the new lap
                }
                else
                {
                    Debug.WriteLine("Race over for player: " + whichPlayer);

                    raceOver = true;
                }
            }

            return raceOver;

        }
    }
}

