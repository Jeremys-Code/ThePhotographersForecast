using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        //public event PropertyChangedEventHandler? PropertyChanged;
        //protected void OnPropertyChanged(string name) =>
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        //        private IDbContextFactory<PfContext> _db;
        [ObservableProperty]
        private ObservableCollection<UnitsModel>? temperatureUnits;

        [ObservableProperty]
        private UnitsModel selectedTemperatureUnit;
        [ObservableProperty]
        private ObservableCollection<UnitsModel>? windSpeedUnits;
        [ObservableProperty]
        private UnitsModel selectedWindSpeedUnit;
        [ObservableProperty]
        private ObservableCollection<UnitsModel>? precipitationUnits;
        [ObservableProperty]
        private UnitsModel selectedPrecipitationUnit;
        [ObservableProperty]
        private string saveStatusMessage;
        [ObservableProperty]
        private bool isSaved;

        public SettingsViewModel()
        {
            IsSaved = false;
            SaveStatusMessage = string.Empty;
            PrecipitationUnits = new ObservableCollection<UnitsModel>
                {
                    new UnitsModel{ UnitName="Inches", UnitAbbreviation="inch" },
                    new UnitsModel{ UnitName="Millimeters", UnitAbbreviation="mm" }
                };

            WindSpeedUnits = new ObservableCollection<UnitsModel>
                {
                    new UnitsModel{ UnitName="Miles/Hour", UnitAbbreviation="mph" },
                    new UnitsModel{ UnitName="Kilometers/Hour", UnitAbbreviation="kmh" },
                    new UnitsModel{ UnitName="Knots", UnitAbbreviation="kn" },
                    new UnitsModel{ UnitName="Meters/Second", UnitAbbreviation="ms" }
                };
            TemperatureUnits = new ObservableCollection<UnitsModel>
                {
                    new UnitsModel{ UnitName="Fahrenheit", UnitAbbreviation="fahrenheit" },
                    new UnitsModel{ UnitName= "Celsius", UnitAbbreviation="celsius" }
                };
            SelectedPrecipitationUnit = PrecipitationUnits.FirstOrDefault(u => u.UnitAbbreviation == Preferences.Get("PrecipitationUnit", "inch")) ?? PrecipitationUnits.First();
            SelectedWindSpeedUnit = WindSpeedUnits.FirstOrDefault(u => u.UnitAbbreviation == Preferences.Get("WindSpeedUnit", "mph")) ?? WindSpeedUnits.First();
            SelectedTemperatureUnit = TemperatureUnits.FirstOrDefault(u => u.UnitAbbreviation == Preferences.Get("TemperatureUnit", "fahrenheit")) ?? TemperatureUnits.First();


        }
        [RelayCommand]
        public void SavePreferences()
        {
            Preferences.Set("TemperatureUnit", SelectedTemperatureUnit.UnitAbbreviation);
            Preferences.Set("WindSpeedUnit", SelectedWindSpeedUnit.UnitAbbreviation);
            Preferences.Set("PrecipitationUnit", SelectedPrecipitationUnit.UnitAbbreviation);
            SaveStatusMessage = "Your settings are saved!";
            // TODO: Use a navigation service to navigate to the AddressPage.
            //MainThread.BeginInvokeOnMainThread(async () =>
            //{
            //    await Shell.Current.Navigation.PushAsync(new AddressPage());
            //});

        }

    }
}
