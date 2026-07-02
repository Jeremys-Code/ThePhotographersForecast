using System;
using System.Collections.Generic;
using System.Text;

namespace ThePhotographersForecast.Services
{
    public class NavigationMessenger
    {
        public string TargetPage { get; }
        public object Parameter { get; }


        public NavigationMessenger(string targetPage, object parameter = null)
        {
            TargetPage = targetPage;
            Parameter = parameter;
        }
    }
}
