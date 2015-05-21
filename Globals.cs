using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlotCar
{
    public enum RaceState { Waiting, Starting, Running, Over, Resetting };
    public enum Player { Lane1, Lane2 };
    public enum CarPosition { Straight1, Turn1, Straight2, Turn2, OffTrack }

    public static class Globals
    {
        static public MainPage theMainPage;

        public static readonly RaceController theRaceController = new RaceController();
        public static readonly TrackSensors theTrackSensors = new TrackSensors();

    }

}
