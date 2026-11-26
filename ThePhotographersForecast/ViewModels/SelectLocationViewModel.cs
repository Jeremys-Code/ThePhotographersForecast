using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.Foldable;
using SkiaSharp.Extended.UI.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;
using ThePhotographersForecast.Services.ML;
using ThePhotographersForecast.Services.Modules;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels
{
    public partial class SelectLocationViewModel : ObservableObject
    {
        //private event EventHandler? DataCollectionCompleted;
        private CancellationTokenSource? _cts;
        private CancellationTokenSource? _searchCts;
        // Use shared _session store's GfsData and EventsData to avoid shadowing and ensure persistence
        [ObservableProperty]
        public partial string SearchText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool IsSearchReadOnly { get; set; }

        [ObservableProperty]
        public partial List<PhotoLocation> Suggestions { get; set; }

        [ObservableProperty]
        public partial bool ShowSuggestions { get; set; }
        [ObservableProperty]
        public partial bool ShowForecast { get; set; }


        [ObservableProperty]
        public partial DayViewModel SelectedDay { get; set; }

        // Index synced with CarouselView.Position to keep SelectedDay and Carousel in sync
        [ObservableProperty]
        public partial int SelectedIndex { get; set; }
        [ObservableProperty]
        public partial EventTime[] SelectedDayEvents { get; set; }


        [ObservableProperty]
        public partial double Longitude { get; set; }
        [ObservableProperty]
        public partial double Latitude { get; set; }
        [ObservableProperty]
        public partial bool IsWorking { get; set; }
        [ObservableProperty]
        public partial string ProgressMessage { get; set; }
        [ObservableProperty]
        private TwoPaneViewPriority _activePanePriority = TwoPaneViewPriority.Pane1;
        [ObservableProperty]
        public partial DateTime StartDate { get; set; }
        [ObservableProperty]
        public partial DateTime EndDate { get; set; }
        [ObservableProperty]
        public partial bool IsInvalidDates { get; set; }
        [ObservableProperty]
        public partial string InvalidDates { get; set; }
        [ObservableProperty]
        public partial double Progress { get; set; }
        [ObservableProperty]
        public partial bool IsComplete { get; set; }
        [ObservableProperty]
        public partial string WelcomeGreeting { get; set; }
        [ObservableProperty]
        public partial bool IsStartDateLoaded { get; set; }
        [ObservableProperty]
        public partial bool IsEndDateLoaded { get; set; }
        [ObservableProperty]
        public partial EventModel[] EventsData { get; set; }
        [ObservableProperty]
        public partial EventTime[] EventTimes { get; set; }
        [ObservableProperty]
        public partial WeatherForecast WeatherForecasts { get; set; }
        [ObservableProperty]
        public partial NwsWeatherModel NwsData { get; set; }
        [ObservableProperty]
        public partial NwsTimeOfDayForecast[] TimeOfDayForecasts { get; set; }
        [ObservableProperty]
        public partial SKFileLottieImageSource SelectedTimeOfDaySKWeatherAnimation { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast CurrentForecast { get; set; }
        [ObservableProperty]
        public partial double Completed { get; set; }
        [ObservableProperty]
        public partial double Total { get; set; }
        [ObservableProperty]
        public partial string DisplayName { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<DayViewModel> Days { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<DayPeriod> Periods { get; set; }
        [ObservableProperty]
        public partial PhotoLocation SelectedLocation { get; set; }
        [ObservableProperty]
        public partial CalendarDayModel[] Dates { get; set; }
        [ObservableProperty]
        public partial FutureMoonPhase[] MoonPhases { get; set; }
        [ObservableProperty]
        public partial FutureMoonPhase SelectedMoonPhase { get; set; }
        [ObservableProperty]
        public partial bool StopQuerying { get; set; }
        [ObservableProperty]
        public partial bool FrontFlipped { get; set; } = true;
        [ObservableProperty]
        public partial bool BackFlipped { get; set; } = false;
        [ObservableProperty]
        public partial int Level { get; set; } = -100;
        [ObservableProperty]
        public partial IReadOnlyList<DailyForecastSummary> DailyPhotoScoringSummary { get; set; }
        [ObservableProperty]
        public partial DailyForecastSummary SelectedDayScoringSummary { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<Inference> Scores { get; set; } = new ObservableCollection<Inference>();
        [ObservableProperty]
        public partial Inference SelectedScore { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<(DateTime, string)> BestTimes { get; set; } = new ObservableCollection<(DateTime, string)>();
        [ObservableProperty]
        public partial string SelectedBestTime { get; set; }

        //public int ModelChangesCount
        //{
        //    get => field;
        //    set => SetProperty(ref field, value);
        //}

        public DateTime MinDate { get; } = DateTime.Now.Date;
        public DateTime MaxDate { get; } = DateTime.Now.AddDays(15).Date;

        //UI Input Commands
        //private IGetEventService _eventService;
        // Replaced with 
        private ISession _session;
        private IGetEphemerisService _ephemerisService;
        private IGetWeatherService _weatherService;
        private IGetMoonService _moonService;
        private IGetNwsData _nwsService;
        private IForecastService _forecastService;
        private ISkyColorEngine _skyEngine;
        private IGetDays _daysService;
        private ISkyColorCalculator _sky;
        private readonly HttpClient _client;

        public SelectLocationViewModel(IGetWeatherService weatherService,
            ISession session,
            IGetEphemerisService ephemerisService,
            IGetMoonService moonService,
            IGetNwsData nwsService,
            IForecastService forecastService,
            ISkyColorEngine skyEngine,
            IGetDays daysService,
            ISkyColorCalculator sky,
            IHttpClientFactory httpClientFactory)
        {
            // 3. Properly assign the matching types
            _session = session;
            _weatherService = weatherService;
            _ephemerisService = ephemerisService;
            _nwsService = nwsService;
            _moonService = moonService;
            _forecastService = forecastService;
            _skyEngine = skyEngine;
            _daysService = daysService;
            _sky = sky;
            _client = httpClientFactory.CreateClient("photographer");
            StopQuerying = false;
            WeatherForecasts = new WeatherForecast();
            _session.NwsData = new NwsWeatherModel();
            NwsData = new NwsWeatherModel();
            _session.Days = new ObservableCollection<DayViewModel>();
            Days = new ObservableCollection<DayViewModel>();
            _session.EventsData = Array.Empty<EventModel>();

            //MainThread.BeginInvokeOnMainThread(async () =>
            //{
            //    await VerifyCachePermissions();
            //});

            WelcomeGreeting = $"{TimeOfDayService.TimesOfDayGreeting(DateTime.Now.Hour)}! Welcome to ";
            IsInvalidDates = false;
            StartDate = DateTime.Now.Date;
            EndDate = DateTime.Now.AddDays(6).Date;
            IsStartDateLoaded = true;
            IsEndDateLoaded = true;
            IsComplete = false;
            Total = 5;
            Completed = 0;
        }

        //private async Task VerifyCachePermissions()
        //{
        //    // Setup Cache Directory for Animation files
        //    var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
        //    if (status != PermissionStatus.Granted)
        //    {
        //        status = await Permissions.RequestAsync<Permissions.StorageWrite>();
        //    }
        //    string cacheFile = Path.Combine(FileSystem.Current.CacheDirectory, "meteocons");
        //    if (Directory.Exists(cacheFile))
        //        return;

        //    Directory.CreateDirectory(cacheFile);
        //    if (await FileSystem.AppPackageFileExistsAsync("meteocons/lottie.dat"))
        //    {
        //        var stream = await FileSystem.OpenAppPackageFileAsync("meteocons/lottie.dat");
        //        var read = new StreamReader(stream);
        //        List<string> files = read.ReadToEnd().Split('\n').ToList();
        //        foreach (string file in files)
        //        {
        //            Helper.GetLottieFromRawResources(cacheFile + "/" + file);
        //            Debug.WriteLine($"{cacheFile}/{file} has been added to cache.");
        //        }
        //        return;
        //    }
        //    return;

        //}

        partial void OnFrontFlippedChanged(bool value)
        {
            BackFlipped = !value;
        }
        partial void OnBackFlippedChanged(bool value)
        {
            FrontFlipped = !value;
        }
        public void OnCompletedChanged()
        {
            Completed += 1;
            if (IsWorking == true && Completed > 0)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Progress = Completed / 9;
                });
            }
        }


        partial void OnStartDateChanged(DateTime value)
        {
            if (IsStartDateLoaded == false)
                return;
            IsInvalidDates = true;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await ValidateDates();
            });
        }

        partial void OnEndDateChanged(DateTime value)
        {
            if (IsEndDateLoaded == false)
                return;
            IsInvalidDates = true;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await ValidateDates();
            });
        }

        private async Task ValidateDates()
        {
            if (StartDate > EndDate)
            {
                if (EndDate.AddDays(6).Date <= MaxDate)
                {
                    EndDate = EndDate.AddDays(6).Date;
                }
                else if (StartDate == MaxDate)
                {
                    EndDate = MaxDate;
                }
                else
                {
                    EndDate = MaxDate;
                }

            }

            _session.StartDate = StartDate;
            _session.EndDate = EndDate;

            IsInvalidDates = false;
        }

        [RelayCommand]
        private async Task GetGps()
        {
            Progress = 0;
            Completed = 0;
            IsWorking = true;
            Level = 100;
            StopQuerying = true;
            try
            {
                ProgressMessage = "Getting GPS location...";
                //   MainThread.BeginInvokeOnMainThread(async () =>
                //   {
                var coordinates = await CurrentLocation.VerifyPermissionAsync();
                if (coordinates.HasValue)
                {
                    Longitude = coordinates.Value.Longitude;
                    Latitude = coordinates.Value.Latitude;
                    _session.SelectedLocation.Latitude = Latitude;
                    _session.SelectedLocation.Longitude = Longitude;
                }
                else
                {
                    return;
                }

                Debug.WriteLine($"Latitude: {Latitude}");
                Debug.WriteLine($"Longitude: {Longitude}");
                OnCompletedChanged(); //1. GPS Complete
                ProgressMessage = "Setting Dates...";
                await CollectAndProcessData();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("$ERROR Processing GPS location");
            }
            return;
        }

        private async Task CollectAndProcessData()
        {
            try
            {
                int nbrOfDays = EndDate.Date.Subtract(StartDate.Date).Days + 1;
                var currentDate = StartDate.Date;
                Dates = new CalendarDayModel[nbrOfDays];
                for (int i = 0; i < nbrOfDays; i++)
                {
                    currentDate = currentDate.Date;
                    Dates[i] = new CalendarDayModel
                    {
                        Date = currentDate
                    };
                    currentDate = currentDate.Date.AddDays(1);
                }
                _session.Dates = Dates.ToArray();

                OnCompletedChanged();//2. Setting Dates


                ProgressMessage = $"Fetching forecasts...";

                var nwsResults = await _nwsService.GetWeatherAsync(Latitude, Longitude);
                if (nwsResults == null)
                {
                    Debug.WriteLine("NWS Service returned null.");
                    return;
                }
                NwsData = new NwsWeatherModel(nwsResults);
                _session.NwsData = new NwsWeatherModel(nwsResults);
                Debug.WriteLine($"nwsResults.DailyForecasts count {_session.NwsData.DailyForecasts?.Length ?? 0}");
                Debug.WriteLine($"nwsResults.HourlySupplementals count {_session.NwsData.HourlySupplementals?.Length ?? 0}");

                OnCompletedChanged();//3. Nws Forecasts

                ProgressMessage = "Lining up the Sun, the Moon, and stars...";

                _session.EventsData = await _ephemerisService.GetDailyAstronomicalEventsAsync(Latitude, Longitude, _session.NwsData);
                if (_session.EventsData == null)
                {
                    Debug.WriteLine("Event service returned null.");
                    return;
                }
                EventsData = _session.EventsData.ToArray();
                EventTimes = _session.EventTimes.ToArray();

                OnCompletedChanged();//4. Solar and Lunar events

                ProgressMessage = $"Calling the station in {_session.NwsData.SelectedLocation.City} for the weather...";

                _session.WeatherForecasts = await _weatherService.BuildWeatherForecastsAsync();

                if (_session.WeatherForecasts.DailyForecasts.Length > 0)
                {
                    WeatherForecasts = new WeatherForecast(_session.WeatherForecasts);

                    Debug.WriteLine($"The weather service completed successfully.");

                }
                else
                {
                    Debug.WriteLine("The weather service failed to build forecasts.");
                }
                OnCompletedChanged();//5. Open Meteo Forecasts

                // MoonPhases = await _moonService.GetFutureMoonPhasesAsync(StartDate, _session.NwsData.TimezoneOffsetSeconds);
                //if (MoonPhases != null && MoonPhases.Length > 0)
                //{

                //    SelectedMoonPhase = MoonPhases.FirstOrDefault();
                //    _session.MoonPhases = MoonPhases.ToArray();
                //}


                ProgressMessage = "Building the most sophisticated forecast...";
                if (_session.NwsData.DailyForecasts.Length > 0 && _session.NwsData.HourlyForecasts.Length > 0 && _session.NwsData.HourlySupplementals.Length > 0 && _session.EventsData.Length > 0)
                {
                    var result = await _forecastService.BuildTimeOfDayForecastsAsync(_session.NwsData.HourlyForecasts);
                    if (result != null)
                    {
                        _session.NwsData.TimeOfDayForecasts = result.ToArray();
                    }
                    TimeOfDayForecasts = result.ToArray();
                    if (TimeOfDayForecasts != null && TimeOfDayForecasts.Length > 0)
                    {
                        _session.NwsData.TimeOfDayForecasts = TimeOfDayForecasts.ToArray();
                        Debug.WriteLine($"{TimeOfDayForecasts.Length} Time of Day Forecasts have been built.");
                    }
                    OnCompletedChanged();//6. Nws Time Of Day Forecasts

                    Dictionary<DateOnly, LinearGradientBrush> skyResults = await _skyEngine.GetDailySkyColorGradientsAsync(Latitude, Longitude);
                    if (skyResults != null && skyResults.Count > 0)
                    {
                        for (int c = 0; c < _session.NwsData.DailyForecasts.Length; c++)
                        {
                            var gradient = skyResults.TryGetValue(DateOnly.FromDateTime(_session.NwsData.DailyForecasts[c].StartTime), out var sky) ? sky : null;
                            _session.NwsData.DailyForecasts[c].SkyColor = gradient;
                        }
                    }

                    OnCompletedChanged();//7. SkyColor
                    // Time of Day Forecast are finished processing here:
                }


                if (_session.NwsData.TimeOfDayForecasts != null)
                {
                    ProgressMessage = "Populating intriguing event and forecast models...";
                    Days = new ObservableCollection<DayViewModel>();
                    var daysResult = await _daysService.GetDaysAsync();
                    if (daysResult != null)
                    {
                        Days = daysResult.ToObservableCollection();
                        _session.Days = daysResult.ToObservableCollection();
                    }
                    OnCompletedChanged();//8. DayViewModels
                    
                    ProgressMessage = "All data and calculations have been processed.";
                    //   _session.WeatherForecasts = new WeatherForecast(WeatherForecasts);
                    Debug.WriteLine($"_session.WeatherForecasts Count: {_session.WeatherForecasts.DailyForecasts.Length}");
                    Debug.WriteLine($"_session.WeatherForecasts Hours Count: {_session.WeatherForecasts.DailyForecasts.SelectMany(x => x.HourlyForecasts).Count()}");
                    //_session.Days = new ObservableCollection<DayViewModel>(Days);
                    Debug.WriteLine($"_session.Days Count: {_session.Days.Count}");
                    Debug.WriteLine($"_session.EventModels Count: {_session.EventsData.Length}");
                    // _session.NwsData = new NwsWeatherModel(NwsData);
                    Debug.WriteLine($"_session.NwsData.DailyForecasts Count: {_session.NwsData.DailyForecasts?.Length ?? 0}");
                    Debug.WriteLine($"_session.NwsData.SelectedDailyHourlyForecasts Count: {_session.NwsData.HourlyForecasts?.Length ?? 0}");
                    Debug.WriteLine($"_session.NwsData.HourlySupplementals Count: {_session.NwsData.HourlySupplementals?.Length ?? 0}");
                    //  _session.Dates = Dates.ToArray();
                    Debug.WriteLine($"_session.Dates Count: {_session.Dates.Length}");

                    var SelectedDay = TimeOfDayForecasts.First();
                    OnCompletedChanged();//9. Finished Counting everything
                    if (_session.DailyPhotoScoringSummary != null)
                    {
                        DailyPhotoScoringSummary = _session.DailyPhotoScoringSummary;
                   

                        foreach (var bestDates in DailyPhotoScoringSummary)
                        {
                            //Guard against previous day data
                            if(bestDates.Date < DateOnly.FromDateTime(DateTime.UtcNow.AddSeconds(_session.NwsData.TimezoneOffsetSeconds)))
                            {
                                continue;
                            }
                            BestTimes = new ObservableCollection<(DateTime, string)>();
                            foreach (var bestDate in bestDates.BestHours)
                            {
                                
                                var sb = new StringBuilder();
                                sb.AppendLine($"When: {bestDate.LocalTime:h:mm tt}");
                                sb.AppendLine($"Drone Safety: {Math.Round(bestDate.DroneSafetyScore)}%");
                                sb.AppendLine($"Photo Score: {Math.Round(bestDate.PhotoScore)}%");
                                sb.AppendLine($"Confidence: {bestDate.ConditionConfidence:P0}");
                                sb.AppendLine($"Conditions: {bestDate.Condition}");
                                sb.AppendLine($"Forecast: {bestDate.HumanReadableForecast}");
                                sb.AppendLine($"Blue Hour: {Math.Round(bestDate.BlueHourScore)}%");
                                sb.AppendLine($"Sunrise: {Math.Round(bestDate.SunriseScore)}%");
                                sb.AppendLine($"Sunset: {Math.Round(bestDate.SunsetScore)}%");
                                sb.AppendLine($"Moon: {Math.Round(bestDate.LunarEventScore)}%");
                                sb.AppendLine($"Galaxy: {Math.Round(bestDate.GalaxyScore)}%");
                                BestTimes.Add((bestDate.LocalTime.DateTime, sb.ToString()));
                            }
                            SelectedBestTime = BestTimes.Where(x => x.Item1.Date == SelectedDay.StartTime.Date).Select(y => y.Item2).FirstOrDefault();

                            var preds = bestDates.HourlyPredictions ?? Array.Empty<HourlyForecastPrediction>();
                            float maxBlue = preds.Count > 0 ? preds.Max(p => p.BlueHourScore) : 0f;
                            float maxSunrise = preds.Count > 0 ? preds.Max(p => p.SunriseScore) : 0f;
                            float maxSunset = preds.Count > 0 ? preds.Max(p => p.SunsetScore) : 0f;
                            float maxMoon = preds.Count > 0 ? preds.Max(p => p.LunarEventScore) : 0f;
                            float maxGalaxy = preds.Count > 0 ? preds.Max(p => p.GalaxyScore) : 0f;

                            Scores.Add(new Inference
                            {
                                Date = bestDates.Date,
                                DateString = $"for {bestDates.Date:dddd, MMMM dd, yyyy}",
                                AvgDroneSafetyScore = $"{Math.Round(bestDates.AverageDroneSafetyScore)}%",
                                BlueHourScore = $"{Math.Round(maxBlue)}%",
                                SurnriseScore = $"{Math.Round(maxSunrise)}%",
                                SunsetScore = $"{Math.Round(maxSunset)}%",
                                MoonScore = $"{Math.Round(maxMoon)}%",
                                GalaxyScore = $"{Math.Round(maxGalaxy)}%",
                                BestHours = BestTimes,
                                BestHoursText = string.Join("\n", BestTimes.Select(b => b.Item2.Trim())),
                                DailyScore = $"{Math.Round(bestDates.DailyPhotoScore)}%",
                                DominantCondition = bestDates.DominantCondition.ToString(),
                                DailySummary = bestDates.HumanReadableDailySummary,
                                SkDayIcon = _session.NwsData.DailyForecasts.Where(x => bestDates.Date == DateOnly.FromDateTime(x.StartTime)).OrderBy(y => y.StartTime).FirstOrDefault().SkDailyAnimation
                            });

                            for (int i = 0; i < _session.NwsData.DailyForecasts.Length; i++)
                            {
                                //Guarding against unselected future dates
                                if (bestDates.Date == DateOnly.FromDateTime(_session.NwsData.DailyForecasts[i].StartTime) && _session.NwsData.DailyForecasts[i].IsDaytime == true)
                                {
                                    _session.NwsData.DailyForecasts[i].DailyScoring = bestDates;
                                }
                            }

                            SelectedScore = Scores.FirstOrDefault();
                        }

                        ProgressMessage = "Finished organizing Forecasts and Events.";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetGps: {ex}");
                await Shell.Current.DisplayAlertAsync("Weather Service Error", "An error occurred while retrieving forecast data. Please try again.", "OK");
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsWorking = false;
                    ShowForecast = true;
                });
            }
            //await Shell.Current.GoToAsync("ForecastDetails");
        }
        [RelayCommand]
        private async Task CardTap(NwsTimeOfDayForecast SelectedDay)
        {
            if (SelectedDay == null) return;
            var dailyForecasts = _session.NwsData?.DailyForecasts;
            if (dailyForecasts == null || dailyForecasts.Length == 0)
                return;
            var daily = _session.NwsData.DailyForecasts.
                    Where(t => SelectedDay.StartTime >= t.StartTime && SelectedDay.EndTime <= t.EndTime).FirstOrDefault();
            int dailyIndex = -1;

            // 1. Match the exact SelectedDailyForecast on the tapped DayViewModel if present
            //if (SelectedDay.SelectedDailyForecast != null)
            //{
            //    dailyIndex = Array.FindIndex(dailyForecasts,
            //        d => d.StartTime == SelectedDay.SelectedDailyForecast.StartTime &&
            //             d.EndTime == SelectedDay.SelectedDailyForecast.EndTime);
            //}
            if (SelectedDay != null)
            {
                dailyIndex = _session.NwsData.DailyForecasts.IndexOf(daily);
            }

            // 2. Otherwise match the daytime (6am-6pm) forecast for the tapped DayViewModel's date
            if (dailyIndex < 0)
            {
                dailyIndex = Array.FindIndex(dailyForecasts,
                    d => d.StartTime.Date == SelectedDay.StartTime.Date && d.IsDaytime);
            }

            // 3. Fallback to any forecast matching the tapped date
            if (dailyIndex < 0)
            {
                dailyIndex = dailyForecasts.IndexOf(daily);
                //                    d => d.StartTime.Date == SelectedDay.StartTime.Date);
            }

            if (dailyIndex < 0)
                dailyIndex = 0;

            await Shell.Current.GoToAsync($"//ForecastDetails?dailyIndex={dailyIndex}");
        }


        // Time of Day forecast - Initial grouping of Hourlies into Time of Day groups for procesing 
        // Time of Day Forecasts are the output of this method
        //private async Task<NwsTimeOfDayForecast[]> BuildSuperDuperForecastAsync()
        //{

        //    var nwsTimeOfDayForecast = new List<NwsTimeOfDayForecast>();
        //    //1.Group By Date
        //    try
        //    {
        //        //3.Then process each date's time of day array of hourlies on Forecast Service Line: 
        //        var result = await _forecastService.BuildTimeOfDayForecastsAsync();
        //        if (result != null)
        //        {
        //            TimeOfDayForecasts = result.ToArray();
        //            for (int i = 0; i < NwsData.DailyForecasts.Length; i++)
        //            {

        //                var times = result.Where(x => NwsData.DailyForecasts[i].StartTime <= x.StartTime && NwsData.DailyForecasts[i].EndTime >= x.EndTime).ToArray();
        //                NwsData.DailyForecasts[i].TimeOfDayForecasts = times;
        //            }

        //            var timeOfDayForecasts = nwsTimeOfDayForecast.OrderBy(x => x.StartTime).ToArray();
        //            return timeOfDayForecasts;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Error while BuildingSuperDuperForecast: {ex}");
        //    }

        //    return nwsTimeOfDayForecast.ToArray();
        //}



        // safe get helper (add near top of class)




        //     var od = weatherForecasts.Where(h => h.Date == dh.StartTime.Date).FirstOrDefault().DailyForecasts;
        //     var om = od.HourlyForecasts.Where(t => t.HourlyTime == dh.StartTime).FirstOrDefault();
        //var dp = dh.StartTime.Hour switch
        //{

        //    >= 0 and < 6 => new DayPeriod
        //    {

        //    },
        //    >= 6 and < 12 => new DayPeriod
        //    {
        //        TimeOfDayName = $"Morning",
        //        Time = dh.StartTime,
        //        HourlyName = dh.TimeOfDayName,
        //        TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodMorning"],
        //        TimeOfDaySkAnimation = Helper.GetLottieFromRawResources("time_late_morning.json"),
        //        TimeOfDayDetailForecast = dh.DetailedForecast,
        //        TimeOfDaySkWeatherAnimation = dh.SkHourlyAnimation,
        //        TimeOfDayStormSummary = GetStormSummary(dh),
        //        TimeOfDayShortForecast = dh.ShortForecast,
        //        TimeOfDayApparentTemperature = dh.Supplemental?.ApparentTemperature != null ? $"{(int)Math.Round(dh.Supplemental.ApparentTemperature.Value, 0)}{dh.Supplemental.ApparentTemperature.UnitCode}" : "",
        //        TimeOfDayDewpoint = $"{(int)Math.Round(dh.DewPoint.Value, 0)}{dh.DewPoint.UnitCode}",
        //        TimeOfDayGust = dh.WindGust != null ? $"{(int)Math.Round(dh.WindGust.Value, 0)}{dh.WindGust.UnitCode}" : "",
        //        TimeOfDayWind = dh.WindStatement,
        //        TimeOfDayTemperature = dh.TempAndUnit,
        //        Events = new ObservableCollection<EventTime>(EventTimes.Where(a => a.AstroTime.Date == dh.StartTime.Date && a.TimeOfDay == "Morning"))
        //    },
        //    >= 12 and < 18 =>
        //    new DayPeriod
        //    {
        //        TimeOfDayName = $"Afternoon",
        //        Time = dh.StartTime,
        //        HourlyName = dh.TimeOfDayName,
        //        TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodAfternoon"],
        //        TimeOfDaySkAnimation = Helper.GetLottieFromRawResources("time_afternoon.json"),
        //        TimeOfDayDetailForecast = dh.DetailedForecast,
        //        TimeOfDaySkWeatherAnimation = dh.SkHourlyAnimation,
        //        TimeOfDayStormSummary = GetStormSummary(dh),
        //        TimeOfDayShortForecast = dh.ShortForecast,
        //        TimeOfDayApparentTemperature = dh.Supplemental?.ApparentTemperature != null ? $"{(int)Math.Round(dh.Supplemental.ApparentTemperature.Value, 0)}{dh.Supplemental.ApparentTemperature.UnitCode}" : "",
        //        TimeOfDayDewpoint = $"{(int)Math.Round(dh.DewPoint.Value, 0)}{dh.DewPoint.UnitCode}",
        //        TimeOfDayGust = dh.WindGust != null ? $"{(int)Math.Round(dh.WindGust.Value, 0)}{dh.WindGust.UnitCode}" : "",
        //        TimeOfDayWind = dh.WindStatement,
        //        TimeOfDayTemperature = dh.TempAndUnit,
        //        Events = new ObservableCollection<EventTime>(EventTimes.Where(a => a.AstroTime.Date == dh.StartTime.Date && a.TimeOfDay == "Afternoon"))
        //    },
        //    >= 18 and < 22 =>
        //    new DayPeriod
        //    {
        //        TimeOfDayName = $"Evening",
        //        Time = dh.StartTime,
        //        HourlyName = dh.TimeOfDayName,
        //        TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodEvening"],
        //        TimeOfDaySkAnimation = Helper.GetLottieFromRawResources("time_evening.json"),
        //        TimeOfDayDetailForecast = dh.DetailedForecast,
        //        TimeOfDaySkWeatherAnimation = dh.SkHourlyAnimation,
        //        TimeOfDayStormSummary = GetStormSummary(dh),
        //        TimeOfDayShortForecast = dh.ShortForecast,
        //        TimeOfDayApparentTemperature = dh.Supplemental?.ApparentTemperature != null ? $"{(int)Math.Round(dh.Supplemental.ApparentTemperature.Value, 0)}{dh.Supplemental.ApparentTemperature.UnitCode}" : "",
        //        TimeOfDayDewpoint = $"{(int)Math.Round(dh.DewPoint.Value, 0)}{dh.DewPoint.UnitCode}",
        //        TimeOfDayGust = dh.WindGust != null ? $"{(int)Math.Round(dh.WindGust.Value, 0)}{dh.WindGust.UnitCode}" : "",
        //        TimeOfDayWind = dh.WindStatement,
        //        TimeOfDayTemperature = dh.TempAndUnit,
        //        Events = new ObservableCollection<EventTime>(EventTimes.Where(a => a.AstroTime.Date == dh.StartTime.Date && a.TimeOfDay == "Evening"))
        //    },
        //    >= 22 and < 24 =>
        //    new DayPeriod
        //    {
        //        TimeOfDayName = $"Night",
        //        Time = dh.StartTime,
        //        HourlyName = dh.TimeOfDayName,
        //        TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodNight"],
        //        TimeOfDaySkAnimation = Helper.GetLottieFromRawResources("time_night.json"),
        //        TimeOfDayDetailForecast = dh.DetailedForecast,
        //        TimeOfDaySkWeatherAnimation = dh.SkHourlyAnimation,
        //        TimeOfDayStormSummary = GetStormSummary(dh),
        //        TimeOfDayShortForecast = dh.ShortForecast,
        //        TimeOfDayApparentTemperature = dh.Supplemental?.ApparentTemperature != null ? $"{(int)Math.Round(dh.Supplemental.ApparentTemperature.Value, 0)}{dh.Supplemental.ApparentTemperature.UnitCode}" : "",
        //        TimeOfDayDewpoint = $"{(int)Math.Round(dh.DewPoint.Value, 0)}{dh.DewPoint.UnitCode}",
        //        TimeOfDayGust = dh.WindGust != null ? $"{(int)Math.Round(dh.WindGust.Value, 0)}{dh.WindGust.UnitCode}" : "",
        //        TimeOfDayWind = dh.WindStatement,
        //        TimeOfDayTemperature = dh.TempAndUnit,
        //        Events = new ObservableCollection<EventTime>(EventTimes.Where(a => a.AstroTime.Date == dh.StartTime.Date && a.TimeOfDay == "Night"))
        //    },
        //    _ => new DayPeriod()



        // Called when the generated SelectedIndex property changes (e.g., user swipes the CarouselView)


        // Called when SelectedDay is changed programmatically - update SelectedIndex so the Carousel moves
        partial void OnSelectedDayChanged(DayViewModel oldValue, DayViewModel newValue)
        {
            if (Days != null && newValue != null)
            {
                int idx = Days.IndexOf(newValue);
                if (idx >= 0 && SelectedIndex != idx)
                {
                    SelectedIndex = idx;
                }
            }
        }
        partial void OnSelectedIndexChanged(int oldValue, int newValue)
        {

            if (Days != null && newValue >= 0 && newValue < Days.Count)
            {
                var newSel = Days[newValue];
                if (!EqualityComparer<DayViewModel>.Default.Equals(SelectedDay, newSel))
                {
                    SelectedDay = newSel;
                }
            }
        }

        [RelayCommand]
        private async Task SelectedSuggestion(PhotoLocation parameter)
        {

            IsWorking = true;
            Level = 100;
            StopQuerying = true;
            try
            {
                if (parameter is PhotoLocation selectedLocation)
                {
                    ProgressMessage = "Setting your selected location...";
                    SelectedLocation = new PhotoLocation(selectedLocation);
                    Latitude = selectedLocation.Latitude;
                    Longitude = selectedLocation.Longitude;
                    DisplayName = selectedLocation.DisplayName;
                    SearchText = selectedLocation.DisplayName;
                    ShowSuggestions = false;
                }
                else
                {
                    return;
                }
                Debug.WriteLine($"Latitude: {Latitude}");
                Debug.WriteLine($"Longitude: {Longitude}");
                OnCompletedChanged();//1. Get Location.
                ProgressMessage = "Setting Dates...";
                await CollectAndProcessData();
            }
            catch (Exception ex) { Debug.WriteLine("$ERROR Processing GPS location"); }
            return;
        }

        //private async Task<List<string>> EnrichForecastAsync(NwsDailyForecast[] dailys)
        //{

        //    var enriched = new List<string>();

        //    for (int e = 0; e < dailys.Length; e++)
        //    {
        //        try
        //        {
        //            var enrichedForecast = await _forecastService.BuildDailyForecastAsync(dailys[e]).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        //            enriched.Add(enrichedForecast);
        //            dailys[e].EnrichedForecast = enrichedForecast;
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.WriteLine("THE NEXT LINE IS WICED IMPORTANT MAKE SURE YOU FIND IT!!!!!!!!!");
        //            Debug.WriteLine($"Enrichment Failed: processing {dailys[e].StartTime.ToString("F")}");
        //        }
        //    }

        //    return enriched;
        //}

        //Part 2: Mapsui static method for turning the geographic coordinates into a layer
        //public static ILayer CreatePolygonLayer(List<(double Lon, double Lat)> coords)
        //{
        //    if (coords == null || coords.Count == 0)
        //        return new MemoryLayer { Name = "NWS Polygon", Features = new GeometryFeature[0] };

        //    // Convert lon/lat to Spherical Mercator (EPSG:3857) coordinates and build NTS Coordinates
        //    var ntsCoords = coords.Select(c =>
        //    {
        //        var merc = SphericalMercator.FromLonLat(c.Lon, c.Lat);
        //        return new NetTopologySuite.Geometries.Coordinate(merc.x, merc.y);
        //    }).ToArray();

        //    if (ntsCoords.Length < 3)
        //        return new MemoryLayer { Name = "NWS Polygon", Features = new GeometryFeature[0] };

        //    // Ensure ring is closed (first == last)
        //    if (!ntsCoords.First().Equals2D(ntsCoords.Last()))
        //    {
        //        var list = ntsCoords.ToList();
        //        list.Add(new NetTopologySuite.Geometries.Coordinate(ntsCoords.First().X, ntsCoords.First().Y));
        //        ntsCoords = list.ToArray();
        //    }

        //    var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 3857);
        //    var shell = geometryFactory.CreateLinearRing(ntsCoords);
        //    var polygon = geometryFactory.CreatePolygon(shell);

        //    var feature = new GeometryFeature { Geometry = polygon };
        //    feature.Styles.Add(new VectorStyle
        //    {
        //        Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.FromArgb(80, 0, 120, 255)), // translucent blue
        //        Outline = new Mapsui.Styles.Pen(Mapsui.Styles.Color.DarkBlue, 2)
        //    });

        //    return new MemoryLayer
        //    {
        //        Name = "NWS Polygon",
        //        Features = new[] { feature }
        //    };
        //}
        // Continue to the BackgroundMapService


        // Creating groups for hours and days
        //    private async Task CreateDailiesHourliesAsync()
        //      {
        //        return;
        //      var daily = _session.WeatherForecasts.Where(w => w.Time == w.StartTime.Time).FirstOrDefault();
        //    if (daily == null) return null;
        //for (int i = 0; i < _session.NwsData.SelectedDailyHourlyForecasts.Length; i++)
        //{
        //    var nwsHourly = _session.NwsData.SelectedDailyHourlyForecasts[i];
        //    var weatherHourly = _session.WeatherForecasts.Where(f => f.Time == nwsHourly.StartTime.Time).SingleOrDefault().DailyForecasts.SelectedDailyHourlyForecasts.Where(d => d.HourlyTime == nwsHourly.StartTime).SingleOrDefault<HourlyForecast>();
        //    var hourlies = new MappedHourly
        //    {
        //        StartTime = nwsHourly.StartTime,
        //        EndTime = nwsHourly.EndTime,
        //        Temperature = new TemperatureValue { Value = Math.Round((nwsHourly.Temperature + weatherHourly.Temperature2m.Value) / 2, 0) },
        //        SkHourlyAnimation = nwsHourly.SkHourlyAnimation,
        //        BriefForecast = nwsHourly.ShortForecast,
        //        GustsStatement = _forecastService.GustsStatement(nwsHourly.Supplemental.WindGust.Value ?? weatherHourly.WindGusts10m.Value)

        //        // TODO run the parts of the forecast through the feelslikeEngine, WeatherReporter and put strings together.
        //        // Look for repetition. if there is, start and end hours, if there isn't, commas between brief forecasts.





        //    });
        //return;
        //  }

        //Shared Helper
        private void ResetCancellation()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }
        partial void OnStopQueryingChanged(bool value)
        {
            if (value)
            {
                IsSearchReadOnly = true;
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            if (IsWorking)
            {
                if (!string.IsNullOrEmpty(DisplayName) && value != DisplayName)
                {
                    SearchText = DisplayName;
                }
                return;
            }

            if (StopQuerying)
            {
                IsSearchReadOnly = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(value) || value.Length < 3)
            {
                _searchCts?.Cancel();
                ShowSuggestions = false;
                return;
            }

            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            string query = value;
            Debounce(async () =>
            {
                await SearchLocationsAsync(query, token);
            }, TimeSpan.FromMilliseconds(900));
        }

        [RelayCommand]
        private void EnableLocationSearch()
        {
            StopQuerying = false;
            IsSearchReadOnly = false;
        }

        private async Task SearchLocationsAsync(string input, CancellationToken token)
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Suggestions = new List<PhotoLocation>();
                    ShowSuggestions = false;
                });

                string escaped = Uri.EscapeDataString(input);
                string apiString = $@"https://nominatim.openstreetmap.org/search?q={escaped}&format=geocodejson&addressdetails=1&countrycodes=us";
                GeocodeResult.Rootobject? results = null;
                try
                {
                    results = await _client.GetFromJsonAsync<GeocodeResult.Rootobject>(apiString, cancellationToken: token);
                }
                catch (IOException ex) when (ex.InnerException is SocketException se && se.SocketErrorCode == SocketError.OperationAborted) { }

                if (results == null || token.IsCancellationRequested)
                    return;

                var newSuggestions = new List<PhotoLocation>();
                if (results.features != null)
                {
                    for (int i = 0; i < results.features.Length; i++)
                    {
                        var result = results.features[i];
                        newSuggestions.Add(new PhotoLocation
                        {
                            Longitude = double.Parse(result.geometry.coordinates[0].ToString()),
                            Latitude = double.Parse(result.geometry.coordinates[1].ToString()),
                            City = result.properties.geocoding.name ?? result.properties.geocoding.city,
                            County = result.properties.geocoding.county,
                            State = result.properties.geocoding.state,
                            Country = result.properties.geocoding.country,
                            GeocodeDisplayName = result.properties.geocoding.label
                        });
                    }
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Suggestions = newSuggestions;
                    ShowSuggestions = Suggestions.Count > 0;
                });
            }
            catch (OperationCanceledException)
            {
                // cancelled - ignore
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error searching locations: {ex.Message}");
            }
        }

        public void Debounce(Func<Task> action, TimeSpan delay)
        {
            // Cancel any pending execution
            _cts?.Cancel();
            _cts?.Dispose();

            // Create a new CTS for the new debounce cycle
            _cts = new CancellationTokenSource();
            var token = _cts.Token;


            // Run the delayed task
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay, token); // Wait for inactivity
                    if (!token.IsCancellationRequested)
                    {
                        await action();
                    }
                }
                catch (TaskCanceledException e)
                {
                    Debug.WriteLine(e);
                    return;

                    // Expected when a new event occurs before delay ends
                }
            }, token);
        }
    }
}


//#1 by GPS
//private async Task<bool> AcquireLocationAsync()
//{
//    try
//    {
//        ProgressMessage = "Getting GPS location...";
//        var coordinates = await CurrentLocation.VerifyPermissionAsync();

//        if (coordinates == null)
//            return false;

//        _session.SelectedLocation = new PhotoLocation
//        {
//            Latitude = coordinates.Value.Latitude,
//            Longitude = coordinates.Value.Longitude
//        };

//        Debug.WriteLine($"Latitude: {_session.SelectedLocation.Latitude}");
//        Debug.WriteLine($"Longitude: {_session.SelectedLocation.Longitude}");

//        ProgressMessage = "Setting Dates...";
//        try
//        {
//            await SetDates();
//            Debug.WriteLine("SUCCESSFULLY CREATED THE DATES ARRAY");
//        }
//        catch (Exception ex)
//        {
//            Debug.WriteLine($"ERROR CREATING THE DATES {ex}");
//        }
//        try
//        {
//            await FetchNwsWeatherAsync();
//            Debug.WriteLine("SUCCESSFULLY BUILT THE NWS COLLECTION");
//        }
//        catch (Exception ex)
//        {
//            Debug.WriteLine($"ERROR BUILDING THE NWS COLLECTION {ex}");
//        }
//        try
//        {
//            //       await GetAstronomyEventsAsync();
//            Debug.WriteLine("SUCCESSFULLY BUILT THE ASTRONOMY COLLECTION");
//        }
//        catch (Exception ex)
//        {
//            Debug.WriteLine($"ERROR BUILDING THE ASTRONOMY COLLECTION {ex}");
//        }
//        try
//        {
//            //   await GetWeatherDataAsync();
//            Debug.WriteLine("SUCCESSFULLY BUILT THE METEO COLLECTION");
//        }
//        catch (Exception ex)
//        {
//            Debug.WriteLine($"ERROR BUILDING THE METEO COLLECTION {ex}");
//        }
//        try
//        {
//            await FetchMoonPhasesAsync();
//            Debug.WriteLine("SUCCESSFULLY BUILT THE MOON COLLECTION");
//        }
//        catch (Exception ex)
//        {
//            Debug.WriteLine($"ERROR BUILDING THE MOON COLLECTION {ex}");
//        }

//        IsWorking = false;
//        // Use absolute routing to avoid relative routing errors
//        await Shell.Current.GoToAsync("//Forecast");
//    }
//    catch (Exception ex)
//    {
//        Debug.WriteLine($"Error acquiring GPS: {ex}");
//        return false;
//    }

//    return true;

//}
//#1 by User Input Location
//private async Task SetSelectedSuggestion(PhotoLocation selectedLocation)
//{

//    try
//    {
//        ProgressMessage = "Setting your selected location...";
//        //#1
//        _session.SelectedLocation = selectedLocation;
//        DisplayName = selectedLocation.DisplayName;
//        if (Shell.Current.FindByName("InputLocation") is Entry location)
//        {
//            location.Text = DisplayName;
//            location.IsReadOnly = true;
//        }

//        ProgressMessage = "Setting Dates...";
//        //#3
//        await SetDates();

//        if (_session.Dates.Length > 0)
//        {
//            Completed += 1;
//        }


//    }
//    catch (Exception ex)
//    {
//        Debug.WriteLine($"Error acquiring GPS: {ex}");
//        return;


//    }
//    return;
//}

//#4 Get Nws Weather
//private async Task<NwsWeatherModel> FetchNwsWeatherAsync()
//{
//    try
//    {
//        MainThread.BeginInvokeOnMainThread(async () =>
//        {
//            ProgressMessage = "Fetching weather...";

//            var result = await _nwsService.GetWeatherAsync(
//                _session.SelectedLocation.Latitude,
//                _session.SelectedLocation.Longitude);
//            if (result != null)
//            {
//                _session.NwsData = result;
//                Debug.WriteLine($" added.");



//            }
//        });
//    }
//    catch (Exception ex)
//    {
//        Debug.WriteLine($"Error fetching weather: {ex}");
//        return default;
//    }

//    return default;
//}
////#7 Get Moon data
//private async Task<FutureMoonPhase[]> FetchMoonPhasesAsync()
//{
//    ProgressMessage = "Fetching moon phases...";

//    var moonResults = await _moonService.GetFutureMoonPhasesAsync(_session.StartDate, _session.SelectedTimeZoneOffset);

//    if (moonResults != null)
//    {
//        Debug.WriteLine($"Moon phases count: {moonResults.Length}");
//        return moonResults;
//    }
//    return default;
//}
//#6 Get Open-Meteo Data
//private async Task<(ObservableCollection<WeatherForecasts>, ObservableCollection<DayViewModel>)> FetchDailyWeatherAsync()
//{
//    //try
//    //{
//    await MainThread.InvokeOnMainThreadAsync(async () =>
//      {
//          ProgressMessage = $"Calling the station in {_session.SelectedLocation.City} for the weather...";
//          var result = await _weatherService.BuildWeatherForecastsAsync(eventResults.Item1,eventResults.Item2);
//          if (result.Item1 != null)
//          {
//              _session.WeatherForecasts = new ObservableCollection<WeatherForecasts>(result.Item1);
//              Debug.WriteLine($"WeatherForecasts count {_session.WeatherForecasts.Count}");
//          }
//          if (result.Item2 != null)
//          {
//              _session.Days = new ObservableCollection<DayViewModel>(result.Item2);
//              Debug.WriteLine($"Days count {_session.Days.Count}");
//          }
//          return result;
//      });
//    return default;
//}


//private async Task<ObservableCollection<DateItem>> SetDates()
//{
//    int nbrOfDays = EndDate.Time.Subtract(StartDate.Time).Days + 1;
//    var currentDate = StartDate.Time;
//    var dateItems = new ObservableCollection<DateItem>();
//    for (int i = 0; i < nbrOfDays; i++)
//    {

//        currentDate = currentDate.Time;

//        dateItems[i] = new DateItem
//        {
//            Time = currentDate
//        };
//        currentDate = currentDate.Time.AddDays(1);
//    }
//    return dateItems;
//}
//#5 Get Astronomy Events
//private async Task<ObservableCollection<EventModel>> GetAstronomyEventsAsync()
//{
//    try
//    {
//        ProgressMessage = "Lining up the Sun, the Moon, and stars...";

//        if (_session.Dates != null && _session.Dates.Length > 0)
//        {

//            var result = await _eventService.GetDayEventsAsync();
//            return result;

//        }
//        return default;
//    }
//    catch (Exception ex)
//    {
//        Debug.WriteLine($"ERROR GETTING ASTRONOMY DATA {ex}");
//        return default;
//    }
//}

//#6: Call Weather Service
//private async Task<(ObservableCollection<WeatherForecasts>, ObservableCollection<DayViewModel>)> GetWeatherDataAsync()
//{

//    ProgressMessage = $"Calling the station in {_session.SelectedLocation.City} for the weather...";

//    try
//    {

//        var result = await _weatherService.BuildWeatherForecastsAsync();
//        return result;
//    }
//    catch (Exception ex)
//    {
//        Debug.WriteLine($"[SelectLocationVM] Error getting weather data. Exception: {ex.GetType().Name}: {ex.Message}");
//        Debug.WriteLine($"[SelectLocationVM] StackTrace: {ex.StackTrace}");
//        ProgressMessage = $"Error: {ex.Message}";
//        return default;
//    }
//}


