using CommunityToolkit.Mvvm.ComponentModel;
using ThePhotographersForecast.Services.ML;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels
{
    public partial class TestingPageViewModel : ObservableObject
    {
        private ISession _session;
        public IReadOnlyList<DailyForecastSummary> Inferences { get; set; }

        //public MissMeNot(ISession session)
        //{
        //    _session = session;

        //}

        //public async Task BuildInferenceResultsAsync()
        //{
        //    var inferences = _session.DailyPhotoScoringSummary;
        //    var inferenceGroups = inferences
        //        .OrderBy(x => x.Date)
        //        .Join(
        //            inner: _session.NwsData.HourlySupplementals,
        //            // Normalize both DateTimes to the exact hour so minor second/offset differences don't break equality
        //            outerKeySelector: inf => new DateTime(inf.h.HourlyPredictions.Select(x => x.LocalTime.DateTime.Year), inf.Date.Month, inf.Date.Day, inf.Date.Hour, 0, 0),
        //            innerKeySelector: sup => new DateTime(sup.Time.Year, sup.Time.Month, sup.Time.Day, sup.Time.Hour, 0, 0),
        //            resultSelector: (inf, sup) => new
        //            {
        //                Inference = inf,
        //                Supplemental = sup
        //            })
        //        .GroupBy(paired => paired.Inference.DominantCondition);
        //}
    }
}
