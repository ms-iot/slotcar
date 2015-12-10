using System;
using Windows.Networking;
using System.Diagnostics;
using Windows.Networking.Sockets;
using System.Threading.Tasks;

namespace SlotCar
{
    class CommUDP
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


        internal CommUDP(string port)
        {
            listingOnPort = port;
            socket = new DatagramSocket();
            socket.MessageReceived += Receive;           
        }

        internal async Task StartServer(string ip)
        {
            await socket.BindEndpointAsync(new HostName(ip), listingOnPort);
        }

        private void Receive(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            uint length = args.GetDataReader().UnconsumedBufferLength;
            string pwm = args.GetDataReader().ReadString(length);
            Debug.WriteLine(pwm);
            float speed = float.Parse(pwm.Split('"')[3]);
            speedUpdate(speed);
        }
    }
}
