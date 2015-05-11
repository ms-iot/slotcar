using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;

namespace SlotCar
{
    class CommTCP
    {
        private const uint bufLen = 8192;
        private string listingOnPort;
        private readonly StreamSocketListener sock = null;

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
            sock.ConnectionReceived += async (s, e) => await ProcessRequestAsync(e.Socket);
        }

        internal async Task StartServer()
        {
            await sock.BindServiceNameAsync(listingOnPort);
        }
        private async Task ProcessRequestAsync(StreamSocket socket)
        {
            try
            {
                StringBuilder requestFull = new StringBuilder(string.Empty);
                using (IInputStream input = socket.InputStream)
                {
                    byte[] data = new byte[bufLen];
                    IBuffer buffer = data.AsBuffer();
                    uint dataRead = bufLen;
                    while (dataRead == bufLen)
                    {
                        await input.ReadAsync(buffer, bufLen, InputStreamOptions.Partial);
                        requestFull.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                        dataRead = buffer.Length;
                    }
                }

                if (requestFull.Length == 0)
                {
                    throw (new Exception("Nothing sent"));
                }

                JsonValue jsonValue = JsonValue.Parse(requestFull.ToString());

                speedUpdate((float)jsonValue.GetObject().GetNamedNumber("PWM"));
            }
            catch (Exception e)
            {
                // Server can force shutdown which generates an exception. Spew it.
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }
    }
}
