using SQLite;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Database
{
    public class DataAccess
    {

        public required SQLiteAsyncConnection database { get; set; }

        async Task Init()
        {
            if (database is not null)
                return;

            database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await database.CreateTablesAsync(CreateFlags.None, typeof(Models.GfsDailyForecast), typeof(Models.GfsHourlyForecast),
                typeof(Models.GfsHourlyTimeTemp),
                typeof(Models.EventTime),
                typeof(Models.PhotoLocation),
                typeof(Models.DailyAstronomicalData));
        }

        public async Task<GfsDailyForecast[]> GetDailyForecastsAsync()
        {
            await Init();
            return await database.Table<GfsDailyForecast>().ToArrayAsync();
        }

        public async Task SetDeviceAndSessionAsync()
        {
            await database.Table<AppUri>().FirstOrDefaultAsync(x => x.DeviceId == Constants.DeviceId);
            await database.Table<AppUri>().OrderByDescending(y => y.SessionId).FirstOrDefaultAsync(z => z.SessionId + 1 == Constants.SessionId);
            return;
        }

        public async Task<GfsHourlyForecast[]> GetHourlyForecastsAsync()
        {
            await Init();
            return await database.Table<GfsHourlyForecast>().ToArrayAsync();
        }

        public async Task<int> SaveForecastsAsync(GfsWeatherModel gfsData)
        {
            await Init();
            int count = 0;
            if (gfsData.DailyForecasts.Count() > 0)
            {
                count = +await database.InsertOrReplaceAsync(gfsData.DailyForecasts);
            }
            if (gfsData.HourlyForecasts.Count > 0)
            {
                count = +await database.InsertOrReplaceAsync(gfsData.HourlyForecasts);
            }
            return count;
        }

        public async Task<int> SaveEventsAsync(EventModel eventModel)
        {
            await Init();
            int count = 0;
            if (eventModel.EventTimes.Count > 0)
            {
                count += await database.InsertOrReplaceAsync(eventModel.EventTimes);
            }
            if (eventModel.AstronomicalData.Count > 0)
            {
                count += await database.InsertOrReplaceAsync(eventModel.AstronomicalData);
            }
            return count;
        }
    }
}
