using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlotCar
{
    public class Stig
    {
        Player WhichPlayer;

        float MaxTurn1Speed = .5f;
        float MaxTurn2Speed = .5f;
        float MaxStraight1Speed = .5f;
        float MaxStraight2Speed = .5f;
        float MaxOffTrackSpeed = .25f;

        CarPosition CurrentCarPosition;
        public Stig(Player whichPlayer)
        {
            WhichPlayer = whichPlayer;
        }

        public void Go()
        {
            CurrentCarPosition = CarPosition.Straight1;

            SetSpeed();
        }

        public void UpdatePosition(CarPosition currentPosition)
        {
            CurrentCarPosition = currentPosition;
            SetSpeed();
        }

        void SetSpeed()
        {
            switch (CurrentCarPosition)
            {
                case CarPosition.Straight1:
                    {
                        Globals.theRaceController.SetSpeed(WhichPlayer, MaxStraight1Speed);
                    }break;

                case CarPosition.Turn1:
                    {
                        Globals.theRaceController.SetSpeed(WhichPlayer, MaxTurn1Speed);
                    }
                    break;

                case CarPosition.Straight2:
                    {
                        Globals.theRaceController.SetSpeed(WhichPlayer, MaxStraight2Speed);
                    }
                    break;

                case CarPosition.Turn2:
                    {
                        Globals.theRaceController.SetSpeed(WhichPlayer, MaxTurn2Speed);
                    }
                    break;

                case CarPosition.OffTrack:
                    {
                        Globals.theRaceController.SetSpeed(WhichPlayer, MaxOffTrackSpeed);
                    }
                    break;
            }
        }
    }
}
