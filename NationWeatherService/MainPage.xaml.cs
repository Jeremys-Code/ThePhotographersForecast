namespace NationWeatherService
{
    public partial class MainPage : ContentPage
    {
        private readonly INwsService _nwsService;

        public string DailyUrl { get; set; }
        public string HourlyUrl { get; set; }
        public string SupplementalUrl { get; set; }
        public MainPage(INwsService nwsService)
        {
            InitializeComponent();
            _nwsService = nwsService;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var result = await _nwsService.GetNwsDataAsync(42.3601, -71.0589); // Example coordinates for Boston, MA


                    DailyUrl = result.Item1;
                    HourlyUrl = result.Item2;
                    SupplementalUrl = result.Item3;


                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync("Error", $"Failed to get weather data: {ex.Message}", "OK");
                }
            });
        }


    }
}
