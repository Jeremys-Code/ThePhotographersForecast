using CommunityToolkit.Mvvm.ComponentModel;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels.Compnents
{
    public partial class MoonPhasesCarouselViewViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial FutureMoonPhase[] SelectedUpcomingMoonPhases { get; set; }
        [ObservableProperty]
        public partial FutureMoonPhase SelectedDisplayedMoonPhase { get; set; }

        private readonly ISession _session;
        public MoonPhasesCarouselViewViewModel(ISession session)
        {
            _session = session;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SelectedUpcomingMoonPhases = [.. _session.MoonPhases];
                SelectedDisplayedMoonPhase = SelectedUpcomingMoonPhases[0];
            });

        }
    }
}
