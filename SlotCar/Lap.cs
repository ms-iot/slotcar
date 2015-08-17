using System;

namespace SlotCar
{
    public class Lap
    {
        public const string UndefinedLapTime = "--:--.----";
        public readonly TimeSpan MinimumLapTime = TimeSpan.FromSeconds(1);

        private DateTime StartTime = DateTime.MinValue;
        private DateTime EndTime = DateTime.MinValue;
        private TimeSpan duration = TimeSpan.Zero;

        public TimeSpan Duration { get { return duration; } }

        public string LapTime
        {
            get
            {
                if (StartTime == DateTime.MinValue)
                {
                    return UndefinedLapTime;
                }
                else
                {
                    DateTime end = (EndTime == DateTime.MinValue) ? DateTime.Now : EndTime;
                    duration = end - StartTime;
                    return String.Format("{0,2:00}:{1,2:00}.{2,3:000}", Duration.Minutes, Duration.Seconds, Duration.Milliseconds);
                }
            }
        }


        public bool Update()
        {
            bool lapDone = false;
            if (StartTime == DateTime.MinValue)
            {
                StartTime = DateTime.Now;
            }
            else
            {
                DateTime updateTime = DateTime.Now;
                TimeSpan lapTime = updateTime - StartTime;

                if (lapTime > MinimumLapTime)
                {
                    // Any lap < the Minimum Lap Time is going to be a glitch.
                    duration = lapTime;
                    EndTime = updateTime;
                    lapDone = true;
                }
            }

            return lapDone;
        }
    }
}
