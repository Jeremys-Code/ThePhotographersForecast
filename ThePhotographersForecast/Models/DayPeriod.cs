using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using ThePhotographersForecast.Services.Modules;

namespace ThePhotographersForecast.Models
{
    public class DayPeriod : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        public DateOnly Date { get; set; }
        public string Name { get; set; }
        public ObservableCollection<EventTime> Events { get; set; } = new ObservableCollection<EventTime>();

        private LinearGradientBrush _backgroundBrush = new LinearGradientBrush();
        public LinearGradientBrush BackgroundBrush
        {
            get => _backgroundBrush;
            set
            {
                if (!object.Equals(_backgroundBrush, value))
                {
                    _backgroundBrush = value;
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(BackgroundBrush)));
                }
            }
        }

        private DayPeriod[] BuildDayPeriods(DateOnly date)
        {
            var day = new DayPeriod[6];

            day[0] = new DayPeriod("Overnight")
            {
                Date = date,
                Events = new ObservableCollection<EventTime>()
            };
            day[1] = new DayPeriod("Early Morning")
            {
                Date = date,
                Events = new ObservableCollection<EventTime>()
            };
            day[2] = new DayPeriod("Morning")
            {
                Date = date,
                Events = new ObservableCollection<EventTime>()
            };
            day[3] = new DayPeriod("Afternoon")
            {
                Date = date,
                Events = new ObservableCollection<EventTime>()
            };
            day[4] = new DayPeriod("Evening")
            {
                Date = date,
                Events = new ObservableCollection<EventTime>()
            };
            day[5] = new DayPeriod("Night")
            {
                Date = date,
                Events = new ObservableCollection<EventTime>()

            };

            return day;
        }

        public DayPeriod(string name)
        {
            Name = name;
            // default neutral brush
            BackgroundBrush = CreateBrush("#001122", "#021024");
        }

        private static LinearGradientBrush CreateBrush(string startHex, string endHex)
        {
            var stops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(startHex), 0f),
                    new GradientStop(Color.FromArgb(endHex), 1f)
                };
            return new LinearGradientBrush(stops, new Point(0, 0), new Point(0, 1));
        }
    }

    // Fixed list of day periods
}
