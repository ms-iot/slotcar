using System;
using System.Text;
using Windows.Networking;
using Windows.Networking.Sockets;
using System.Threading.Tasks;

namespace SlotCar
{
    class CommTCP
    {
        private const uint bufLen = 8192;
        private string listingOnPort;
        private readonly DatagramSocket socket = null;

        public delegate void TrackSpeedUpdate(float speed);

        public event TrackSpeedUpdate speedUpdate;

        public event TrackSpeedUpdate SpeedUpdate
        {
            add
            {
                speedUpdate += value;
            }
            remove
            {
                speedUpdate -= value;
            }
        }


        internal CommTCP(string port)
        {
            listingOnPort = port;
            socket = new DatagramSocket();
            socket.MessageReceived += Receive;           
        }

        internal async Task StartServer()
        {
            await socket.BindEndpointAsync(new HostName("localhost"), listingOnPort);
        }

        private void Receive(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            float speed = float.Parse(args.ToString().Split('"')[3]);
            speedUpdate(speed);
        }
    }
}
