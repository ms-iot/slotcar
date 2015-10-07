using System;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Diagnostics;
using Windows.System.Threading;

namespace SlotCar
{
    class CommTCP
    {
        private const uint bufLen = 15;
        private string listingOnPort;
        private readonly StreamSocketListener sock = null;
        private DataReader reader;
        private StreamSocket socket;
        private ThreadPoolTimer timer;

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
            sock = new StreamSocketListener();
            sock.Control.KeepAlive = true;
            listingOnPort = port;
            sock.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
        }

        internal async Task StartServer()
        {
            await sock.BindServiceNameAsync(listingOnPort);
        }
        private void ProcessRequestAsync(StreamSocket streamsocket)
        {
            socket = streamsocket;
            reader = new DataReader(socket.InputStream);
            reader.InputStreamOptions = InputStreamOptions.ReadAhead;
            timer = ThreadPoolTimer.CreatePeriodicTimer(read, TimeSpan.FromMilliseconds(125));
        }

        public async void read(ThreadPoolTimer t)
        {
            timer.Cancel();
            await reader.LoadAsync(bufLen);
            char[] buffer = new char[15];
            char c = 'x';
            for (int i = 0; c != '\n'; ++i)
            {
                c = (char)reader.ReadByte();
                buffer[i] = c;
            }
            Debug.WriteLine(buffer.ToString());
            float speed = float.Parse(buffer.ToString().Split('"')[3]);
            speedUpdate(speed);
            timer = ThreadPoolTimer.CreatePeriodicTimer(read, TimeSpan.FromMilliseconds(125));
        }
    }
}
