using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SlotCarUICS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        string[] columnHeader = { "", "Sensor1", "Sensor2", "Sensor3", "Sensor4", "Sensor5", "Sensor6", "Sensor7", "Sensor8", "Sensor9", "LapTime", "Laps" };
        string[] rowHeader = { "Racer1", "Racer2" };

        List<RacerDataModel> racerData = new List<RacerDataModel>();

        DatagramSocket socket = new DatagramSocket();

        const int sensorDataCount = 4;
        const int numberOfPlayers = 2;
        private string recordLapTime = null;

        public MainPage()
        {
            this.InitializeComponent();

            tableData.Children.Clear();

            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF8, 0xFF));

            for (int i = 0; i < numberOfPlayers; i++)
            {
                RacerDataModel r = new RacerDataModel();
                racerData.Add(r);
            }

            // For all the headers
            for (int i = 0; i < columnHeader.Length; i++)
            {
                Border b = new Border();
                b.BorderBrush = brush;
                b.BorderThickness = new Thickness(2);
                TextBlock t = new TextBlock();
                t.TextAlignment = TextAlignment.Center;
                t.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                t.FontSize = 20;
                t.Text = columnHeader[i];
                b.Child = t;
                Grid.SetRow(b, 0);
                Grid.SetColumn(b, i);
                tableData.Children.Add(b);
            }
            for (int i = 0; i < numberOfPlayers; i++)
            {
                Border b = new Border();
                b.BorderBrush = brush;
                b.BorderThickness = new Thickness(2);
                TextBlock t = new TextBlock();
                t.TextAlignment = TextAlignment.Center;
                t.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                t.FontSize = 20;

                t.Text = rowHeader[i];
                Binding bind = new Binding();
                bind.Path = new PropertyPath("CarColor");
                bind.Source = racerData.ElementAt(i);
                t.SetBinding(TextBlock.ForegroundProperty, bind);

                b.Child = t;
                Grid.SetRow(b, i + 1);
                Grid.SetColumn(b, 0);
                tableData.Children.Add(b);
            }

            for (int j = 0; j < numberOfPlayers; j++)
            {
                for (int i = 0; i < sensorDataCount; i++)
                {
                    Border b = new Border();
                    b.BorderBrush = brush;
                    b.BorderThickness = new Thickness(2);
                    TextBlock t = new TextBlock();
                    t.TextAlignment = TextAlignment.Center;
                    t.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                    t.FontSize = 15;

                    Binding bind = new Binding();
                    bind.Path = new PropertyPath("Data[" + i.ToString() + "]");
                    bind.Source = racerData.ElementAt(j);
                    t.SetBinding(TextBlock.TextProperty, bind);

                    b.Child = t;
                    Grid.SetRow(b, j + 1);
                    Grid.SetColumn(b, i + 1);
                    tableData.Children.Add(b);
                }
            }

            // laptimes
            for (int i = 0; i < 2; i++)
            {
                Border b = new Border();
                b.BorderBrush = brush;
                b.BorderThickness = new Thickness(2);
                TextBlock t = new TextBlock();
                t.TextAlignment = TextAlignment.Center;
                t.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                t.FontSize = 15;

                Binding bind = new Binding();
                bind.Path = new PropertyPath("LapTimeString");
                bind.Source = racerData.ElementAt(i);
                t.SetBinding(TextBlock.TextProperty, bind);

                b.Child = t;
                Grid.SetRow(b, i + 1);
                Grid.SetColumn(b, 10);
                tableData.Children.Add(b);
            }

            // laps
            for (int i = 0; i < 2; i++)
            {
                Border b = new Border();
                b.BorderBrush = brush;
                b.BorderThickness = new Thickness(2);
                TextBlock t = new TextBlock();
                t.TextAlignment = TextAlignment.Center;
                t.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                t.FontSize = 15;

                Binding bind = new Binding();
                bind.Path = new PropertyPath("Laps");
                bind.Source = racerData.ElementAt(i);
                t.SetBinding(TextBlock.TextProperty, bind);

                b.Child = t;
                Grid.SetRow(b, i + 1);
                Grid.SetColumn(b, 11);
                tableData.Children.Add(b);
            }

            PrintDebugString("");
            statusTextBlock.Text = "Runner-up";
            recordTextBlock.Text = "00:00:01";

            this.Listen();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var racer in racerData)
            {
                int data = racer.Data.ElementAt(0);
                racer.setSensorPoint(0, ++data);
                racer.Laps = ++racer.Laps;
                racer.LastLapTriggerTime = DateTime.Now;
                if (recordLapTime == null || recordLapTime.CompareTo(racer.LapTimeString) > 0)
                {
                    recordLapTime = racer.LapTimeString;
                    recordTextBlock.Text = recordLapTime;
                }

                string color = "#1ffc64";
                if (colorTextBox.Text != string.Empty)
                {
                    color = colorTextBox.Text;
                }

                Int32 colorInt = Convert.ToInt32(color.Substring(1), 16);
                byte[] colorBytes = BitConverter.GetBytes(colorInt);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(colorBytes);
                racer.CarColor = new SolidColorBrush(Color.FromArgb(0xFF, colorBytes[1], colorBytes[2], colorBytes[3]));

                //Random rnd = new Random();
                //racer.CarColor = new SolidColorBrush(Color.FromArgb(0xFF, (byte)rnd.Next(), (byte)rnd.Next(), (byte)rnd.Next()));
            }
        }

        private void PrintDebugString(string s)
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                debugOutputTextBlock.Text = debugOutputTextBlock.Text + "\n" + s;

                // Stops the auto-scrolling if you are not at the bottom of the scrollviewer 
                if (debugOutputScrollViewer.VerticalOffset >= (debugOutputTextBlock.ActualHeight - debugOutputScrollViewer.ActualHeight - 50))
                {
                    debugOutputScrollViewer.ChangeView(null, debugOutputTextBlock.ActualHeight, null);
                }
            });
        }

        private void Listen()
        {
            var udp = new UdpSocket();
            udp.OnMessage +=
                message =>
                {
                    this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        JsonObject root;

                        try
                        {
                            root = JsonValue.Parse(message).GetObject();
                        }
                        catch (Exception)
                        {
                            //bad packet?
                            PrintDebugString("-bad packet-");
                            return;
                        }
                         
                        IJsonValue value;
                        int track = -1;
                        int position = -1;
                        string color = string.Empty;

                        if (root.TryGetValue("track", out value))
                        {
                            track = (int)value.GetNumber();
                        }
                        if (root.TryGetValue("position", out value))
                        {
                            position = (int)value.GetNumber();
                        }
                        if (root.TryGetValue("color", out value))
                        {
                            color = value.GetString();
                        }

                        if (root.TryGetValue("status", out value))
                        {
                            statusTextBlock.Text = value.GetString();
                        }

                        if (track >= 0 && position >= 0)
                        {
                            int previous = racerData.ElementAt(track - 1).Data.ElementAt(position);
                            racerData.ElementAt(track - 1).setSensorPoint(position, ++previous);
                        }

                        if (track >= 0 && color != string.Empty)
                        {
                            var racer = racerData[track];

                            Int32 colorInt = Convert.ToInt32(color.Substring(1), 16);
                            byte[] colorBytes = BitConverter.GetBytes(colorInt);
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(colorBytes);
                            racer.CarColor = new SolidColorBrush(Color.FromArgb(0xFF, Amplify(colorBytes[1]), Amplify(colorBytes[2]), Amplify(colorBytes[3])));
                        }
                    });
                    PrintDebugString(message);
                };
            udp.Open();
        }

        static byte Amplify(byte color)
        {
            return (byte) Math.Min(255, color * 4);
        }

        class UdpSocket : IDisposable
        {
            DatagramSocket socket;

            public delegate void MessageHandler(string message);

            public event MessageHandler OnMessage;

            public UdpSocket()
            {

                this.socket = new DatagramSocket();
                this.socket.MessageReceived += socket_MessageReceived;

            }

            private async void socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
            {
                var reader = args.GetDataReader();
                var len = reader.UnconsumedBufferLength;
                var msg = reader.ReadString(len);

                this.OnMessage.Invoke(msg);
            }

            public async Task Open()
            {
                var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
                this.socket.BindServiceNameAsync("12346", connectionProfile.NetworkAdapter);
                //await this.socket.BindEndpointAsync(new HostName("10.125.149.59"), "12345");
                this.socket.JoinMulticastGroup(new HostName("224.0.0.251"));
            }

            public void Dispose()
            {

            }
        }
    }
}
