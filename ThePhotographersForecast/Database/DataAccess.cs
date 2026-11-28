using System;
using System.Collections.Generic;
using System.Text;
using ThePhotographersForecast.Models;
using SQLite;
using Microsoft.Maui.Storage;
using ThePhotographersForecast.Services;
using System.Runtime.CompilerServices;
using ThePhotographersForecast.Shared;
using System.Diagnostics;

namespace ThePhotographersForecast.Database
{
    public class DataAccess
    {
        

        private SQLiteConnection conn;
        private ISession _session;
        public DataAccess(ISession session)
        {
            _session = session;

        }
        public void Initialize()
        {
            string path = Path.Combine(FileSystem.Current.AppDataDirectory, "photographers_forecast.db3");
            if (conn != null)
                return;

            conn = new SQLiteConnection(path);
            conn.CreateTable<ForecastInputs>();
            conn.CreateTable<Layers>();
            conn.CreateTable<ForecastLayers>();
            conn.CreateTable<ForecastWind>();
            conn.CreateTable<ForecastTransportWind>();
            conn.CreateTable<ForecastPrecipitation>();
            conn.CreateTable<WeatherStatementsValue>();
            conn.CreateTable<ForecastShort>();
            return;

        }

        public void LoadForecastInputs()
        {
            Initialize();

            var inputs = _session.ForecastInputsCollection;
            if (inputs == null || inputs.Count == 0)
                return;

            conn.RunInTransaction(() =>
            {
                // 1. Insert ForecastInputs first so AutoIncrement assigns each ForecastId
                var inputCount = conn.InsertAll(inputs);

                // 2. Ensure Layers lookup table has an entry for every standard layer
                var layerIdByName = EnsureLayers(ForecastInputs.LayerNames);

                // 3. Flatten all standard IForecastLayerValue properties into ForecastLayers (Option 2, AOT/iOS-safe)
                var forecastLayers = inputs
                    .SelectMany(fi => fi.GetLayers()
                        .Where(l => l.Data != null)
                        .Select(l => new ForecastLayers
                        {
                            ForecastId = fi.ForecastId,
                            LayerId = layerIdByName[l.Name],
                            StartTime = l.Data!.StartTime,
                            EndTime = l.Data.EndTime,
                            Value = l.Data.Value,
                            UnitCode = l.Data.UnitCode,
                            Duration = (int)(l.Data.EndTime - l.Data.StartTime).TotalHours
                        }))
                    .ToList();

                var layerCount = conn.InsertAll(forecastLayers);

                // 4. Insert the oddball tables with ForecastId linked to each parent ForecastInputs row
                var winds = inputs
                    .Where(fi => fi.Wind != null)
                    .Select(fi =>
                    {
                        fi.Wind.ForecastId = fi.ForecastId;
                        return fi.Wind;
                    })
                    .ToList();

                var transportWinds = inputs
                    .Where(fi => fi.TransportWind != null)
                    .Select(fi =>
                    {
                        fi.TransportWind.ForecastId = fi.ForecastId;
                        return fi.TransportWind;
                    })
                    .ToList();

                var precipitations = inputs
                    .Where(fi => fi.Precipitation != null)
                    .Select(fi =>
                    {
                        fi.Precipitation.ForecastId = fi.ForecastId;
                        return fi.Precipitation;
                    })
                    .ToList();

                var shortForecasts = inputs
                    .Where(fi => fi.ShortForecast != null)
                    .Select(fi =>
                    {
                        fi.ShortForecast.ForecastId = fi.ForecastId;
                        return fi.ShortForecast;
                    })
                    .ToList();

                var weatherStatements = inputs
                    .Where(fi => fi.Weather != null)
                    .SelectMany(fi => fi.Weather
                        .Where(w => w != null)
                        .Select(w =>
                        {
                            w.ForecastId = fi.ForecastId;
                            return w;
                        }))
                    .ToList();

                var windCount = conn.InsertAll(winds);
                var transportWindCount = conn.InsertAll(transportWinds);
                var precipCount = conn.InsertAll(precipitations);
                var shortCount = conn.InsertAll(shortForecasts);
                var weatherCount = conn.InsertAll(weatherStatements);

                Debug.WriteLine(
                    $"SQLite saved: {inputCount} inputs, {layerCount} layers, {windCount} winds, " +
                    $"{transportWindCount} transport winds, {precipCount} precip, {shortCount} short forecasts, {weatherCount} weather statements.");
            });
        }

        private Dictionary<string, int> EnsureLayers(IEnumerable<string> layerNames)
        {
            var existing = conn.Table<Layers>().ToDictionary(l => l.LayerName, l => l.LayerId);

            foreach (var name in layerNames)
            {
                if (!existing.ContainsKey(name))
                {
                    var layer = new Layers { LayerName = name };
                    conn.Insert(layer);
                    existing[name] = layer.LayerId;
                }
            }

            return existing;
        }
    }
}
