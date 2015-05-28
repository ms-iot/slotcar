using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlotCar
{
    public class Stig
    {
        Player WhichPlayer;

        float MaxStartSpeed = .35f;
        float MaxTurn1Speed = .35f;
        float MaxTurn2Speed = .35f;
        float MaxStraight1Speed = .5f;
        float MaxStraight2Speed = .45f;
        float MaxOffTrackSpeed = .35f;

        CarPosition CurrentCarPosition;
        public Stig(Player whichPlayer)
        {
            WhichPlayer = whichPlayer;
        }

        public void Go()
        {
            CurrentCarPosition = CarPosition.Start;

            SetSpeed();
        }

        public void UpdatePosition(CarPosition currentPosition)
        {
            CurrentCarPosition = currentPosition;
            SetSpeed();
        }

        void SetSpeed()
        {
            float newSpeed = 0f;
            switch (CurrentCarPosition)
            {
                case CarPosition.Start:
                    {
                        Debug.WriteLine("Lane {0}, Start speed: ", WhichPlayer + 1);
                        newSpeed = MaxStartSpeed;
                    }
                    break;

                case CarPosition.Straight1:
                    {
                        Debug.WriteLine("Lane {0}, Straight 1 speed: ", WhichPlayer + 1);
                        newSpeed = MaxStraight1Speed;
                    }
                    break;

                case CarPosition.Turn1:
                    {
                        Debug.WriteLine("Lane {0}, Turn1 1 speed: ", WhichPlayer + 1);
                        newSpeed = MaxTurn1Speed;
                    }
                    break;

                case CarPosition.Straight2:
                    {
                        Debug.WriteLine("Lane {0}, Straight 2 speed: ", WhichPlayer + 1);
                        newSpeed = MaxStraight2Speed;
                    }
                    break;

                case CarPosition.Turn2:
                    {
                        Debug.WriteLine("Lane {0}, Turn 2 speed: ", WhichPlayer + 1);
                        newSpeed = MaxTurn2Speed;
                    }
                    break;

                case CarPosition.OffTrack:
                    {
                        Debug.WriteLine("Lane {0}, Offtrack speed: ", WhichPlayer + 1);
                        newSpeed = MaxOffTrackSpeed;
                    }
                    break;
            }
            Globals.theRaceController.SetSpeed(WhichPlayer, newSpeed);
            Debug.WriteLine(newSpeed);

        }
    }
}
