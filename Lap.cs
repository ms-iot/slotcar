using System;

namespace Slotcar
{
    class Lap
    {
        private DateTime StartTime = new DateTime();
        private DateTime EndTime = new DateTime();
        private TimeSpan duration = TimeSpan.Zero;

        public TimeSpan Duration { get { return duration; } }

        public string LapTime
        {
            get
            {
                if (StartTime.Year == 1)
                {
                    return "--:--.----";
                }
                else
                {
                    DateTime end = (EndTime.Year == 1) ? DateTime.Now : EndTime;
                    duration = end - StartTime;
                    return String.Format("{0,2:00}:{1,2:00}.{2,3:000}", Duration.Minutes, Duration.Seconds, Duration.Milliseconds);
                }
            }
        }

        public bool Update()
        {
            bool lapDone = false;
            if (StartTime.Year == 1)
            {
                StartTime = DateTime.Now;
            }
            else
            {
                EndTime = DateTime.Now;
                duration = EndTime - StartTime;
                lapDone = true;
            }

            return lapDone;
        }
    }
}
