using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlotCar
{
    public enum RaceState { Waiting, Starting, Running, Over };
    public enum Player { Lane1, Lane2 };
    public enum CarPosition { Start, Turn1, Straight1, Turn2, Straight2, OffTrack }

    public static class Globals
    {
        static public MainPage theMainPage;

        public static readonly RaceController theRaceController = new RaceController();
        public static readonly TrackSensors theTrackSensors = new TrackSensors();

    }

}
