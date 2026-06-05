using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ThePhotographersForecast.ViewModels.ViewModels
{
    partial class DayViewModel : ObservableObject
    {
        [ObservableProperty]
        private string name;
        [ObservableProperty]
        private string timeOfDay;
        [ObservableProperty]
        private DateTime startTime;
        [ObservableProperty]
        private DateTime endTime;
        [ObservableProperty]
        private int _day;
        [ObservableProperty]
        private bool _isEventDay;
        [ObservableProperty]
        private bool _isEventWindowDay;
        [ObservableProperty]
        private bool _isToday;
        [ObservableProperty]
        private bool _isEmpty;
        [ObservableProperty]
        private bool _isFlipped;
        [ObservableProperty]
        private string detailForecast;
        [ObservableProperty]
        private string icon;
    }
}