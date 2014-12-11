using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace SlotCarUICS
{
    class RacerDataModel : INotifyPropertyChanged
    {
        private List<int> arrayOfSensors;
        private DateTime lastLapTriggerTime;
        private TimeSpan lapTime;
        private string lapTimeString;
        private int laps;
        private SolidColorBrush carColor;

        private const int defaultSensorCount = 9;

        public RacerDataModel()
            : this(defaultSensorCount)
        {
        }

        public RacerDataModel(int sensorCount)
        {
            arrayOfSensors = new List<int>(sensorCount);
            for (int i = 0; i < sensorCount; i++)
            {
                arrayOfSensors.Add(i);
            }
            lastLapTriggerTime = DateTime.Now;
            lapTime = new TimeSpan();
            lapTimeString = "00:00:00";
            laps = 0;
            carColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0x00, 0xFF));
        }

        public List<int> Data
        {
            get { return arrayOfSensors; }
        }

        public void setSensorPoint(int index, int value)
        {
            int temp = arrayOfSensors.ElementAt(index);
            temp = value;
            arrayOfSensors.RemoveAt(index);
            arrayOfSensors.Insert(index, temp);
            OnPropertyChanged("Data");
        }

        public DateTime LastLapTriggerTime
        {
            get { return lastLapTriggerTime; }
            set
            {
                CalculateAndAssignLapTime(lastLapTriggerTime, value);
                lastLapTriggerTime = value;
                OnPropertyChanged("LapTimeString");
            }
        }

        public int Laps
        {
            get { return laps; }
            set
            {
                laps = value;
                OnPropertyChanged("Laps");
            }
        }

        public String LapTimeString
        {
            get { return lapTimeString; }
        }

        public SolidColorBrush CarColor
        {
            get { return carColor; }
            set
            {
                carColor = value;
                OnPropertyChanged("CarColor");
            }
        }

        private void CalculateAndAssignLapTime(DateTime oldDate, DateTime newDate)
        {
            TimeSpan ts = newDate - oldDate;
            lapTime = ts;
            lapTimeString = ts.Minutes + ":" + ts.Seconds + ":" + ts.Milliseconds;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
