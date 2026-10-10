# Fix Weather Service Crash on Android

## Problem Analysis
Investigation of device logcat from the connected Android test device (`49121FDKD001Z4`) revealed the exact crash during weather service execution:

```
FATAL EXCEPTION: main
Process: com.bostonfromabove.thephotographersforecast, PID: 15809
android.runtime.JavaProxyThrowable: [System.InvalidOperationException]: Collection was modified; enumeration operation may not execute.
	at System.Collections.Generic.List`1+Enumerator.MoveNext()
	at System.Linq.Enumerable.FirstOrDefault()
	at ThePhotographersForecast.Services.WmoLookup.GetWeather(WmoLookup.cs:45)
	at ThePhotographersForecast.Services.GetWeatherService.GetHourlyAsync(GetWeatherService.cs:222)
	at ThePhotographersForecast.Services.GetWeatherService.BuildWeatherForecastsAsync(GetWeatherService.cs:91)
	at ThePhotographersForecast.ViewModels.SelectLocationViewModel.GetGps(SelectLocationViewModel.cs:220)
```

### Root Causes
1. **Thread Safety & Race Condition in [`WmoLookup.cs`](file:///F:/source/ThePhotographersForecast/ThePhotographersForecast/Services/WmoLookup.cs#L34-L46)**:
   - When [`GetWeather()`](file:///F:/source/ThePhotographersForecast/ThePhotographersForecast/Services/WmoLookup.cs#L34-L59) is called, if `WmoCodesList` is null or empty, it launches `Task.Run(async () => await ImportWmoCodes())` in the background without awaiting.
   - When [`GetHourlyAsync()`](file:///F:/source/ThePhotographersForecast/ThePhotographersForecast/Services/GetWeatherService.cs#L212-L285) iterates through all 168 forecast hours in rapid succession, it calls `WmoLookup.GetWeather()` repeatedly.
   - The background thread calls `WmoCodesList.Add(...)` while subsequent calls on the caller thread call `WmoCodesList.Where(...).FirstOrDefault()`.
   - Mutating a standard `List<T>` while enumerating it throws `System.InvalidOperationException: Collection was modified; enumeration operation may not execute`.
   - In addition, because `WmoCodesList.Count` remains 0 until `ImportWmoCodes()` completes, many dozens of concurrent background tasks are spawned simultaneously, causing severe thread contention and unhandled exceptions.

2. **Unsafe Nullable and Null Dereferences in [`GetWeatherService.cs`](file:///F:/source/ThePhotographersForecast/ThePhotographersForecast/Services/GetWeatherService.cs)**:
   - Line 232: `item.hourly.snow_depth[h].Value.ToString()` accesses `.Value` on a nullable `double?`. If null, this throws `InvalidOperationException: Nullable object must have a value`.
   - Line 111: `(double)item.elevation` will throw `InvalidOperationException` if `elevation` is null.
   - Line 109: `TimeZoneInfo.FindSystemTimeZoneById(timeZone)` throws `TimeZoneNotFoundException` on Android if the IANA identifier is not mapped.
   - Line 320, 330: In `CreateDayViewModel`, `dailyForecast.DailyEvents.EventTimes` and summaries dereference properties without null checks. If an event is not found for that date or if a summary is null, it throws `NullReferenceException`.
   - `BuildWeatherForecastsAsync` had its `try-catch` commented out, allowing any exception to crash Android's main loop.

3. **GPS Location Never Passed to Session in [`SelectLocationViewModel.cs`](file:///F:/source/ThePhotographersForecast/ThePhotographersForecast/ViewModels/SelectLocationViewModel.cs#L170-L176)**:
   - In `GetGps()`, `Session.SelectedLocation` was commented out. `Latitude` and `Longitude` were retrieved from GPS but never assigned to `Session.SelectedLocation`.
   - As a result, subsequent calls to `ipgeolocation.io` and `open-meteo.com` queried coordinates `0, 0` instead of the user's actual location.
   - Missing `try-catch` blocks and missing `finally { IsWorking = false; }` in `GetGps()` and `SelectedSuggestion()` caused the UI to hang indefinitely when any error occurred.

4. **Missing Android Permission in [`AndroidManifest.xml`](file:///F:/source/ThePhotographersForecast/ThePhotographersForecast/Platforms/Android/AndroidManifest.xml)**:
   - Android 12+ requires `android.permission.ACCESS_COARSE_LOCATION` whenever `ACCESS_FINE_LOCATION` is requested.

---

## User Review Required

> [!NOTE]
> `WmoLookup` will be refactored to use a pre-populated static lookup dictionary containing the 28 standard WMO weather codes (for both day and night), with thread-safe lazy CSV fallback. This guarantees zero race conditions and instant O(1) lookups during the 168-hour forecast processing loop.

---

## Proposed Changes

### Core Services & Lookup

#### [MODIFY] [WmoLookup.cs](file:///F:/source/ThePhotographersForecast/ThePhotographersForecast/Services/WmoLookup.cs)
- Implement a thread-safe, immutable dictionary of WMO weather codes embedded directly in the class so it is instantly available with no async race conditions.
- Keep `WmoCodesList` as an `IReadOnlyList<WmoCode>` for backwards compatibility.
- Ensure `GetWeather(int weatherCode, int isDay)` provides instant, O(1) thread-safe lookup without mutating collections or spawning unawaited tasks.

#### [MODIFY] [wmocodes.csv](file:///F:/source/ThePhotographersForecast/ThePhotographersForecast/Resources/Raw/wmocodes.csv)
- Fix typo on line 20: replace period with comma (`"nfzra.png"."mostly-clear-night-drizzle.json"` -> `"nfzra.png","mostly-clear-night-drizzle.json"`).

---

### Weather Service Resilience

#### [MODIFY] [GetWeatherService.cs](file:///F:/source/ThePhotographersForecast/ThePhotographersForecast/Services/GetWeatherService.cs)
- Fix nullable `snow_depth`: change `Double.TryParse(item.hourly.snow_depth[h].Value.ToString(), out double d) ? d : double.NaN` to `item.hourly.snow_depth[h] ?? 0.0`.
- Fix nullable `elevation`: use `item.elevation ?? 0.0`.
- Safely resolve `TimeZoneInfo`: wrap `TimeZoneInfo.FindSystemTimeZoneById` with fallback to `TimeZoneInfo.Local`.
- Add null-conditional navigation in `CreateDayViewModel`:
  - `dailyForecast.DailyEvents?.EventTimes?.Where(...) ?? ...`
  - `dailyForecast.DailyForecastSummary?.WeatherSummary ?? string.Empty`
  - `wh.HourlyForecastSummary?.WeatherSummary ?? string.Empty`
- Re-enable and enhance `try-catch` logging in `BuildWeatherForecastsAsync` so errors return `false` cleanly and do not crash the app.

---

### ViewModel & Platform Configuration

#### [MODIFY] [SelectLocationViewModel.cs](file:///F:/source/ThePhotographersForecast/ThePhotographersForecast/ViewModels/SelectLocationViewModel.cs)
- In `GetGps()`, ensure `Session.SelectedLocation` is updated with `Latitude` and `Longitude`:
  ```csharp
  Session.SelectedLocation.Latitude = Latitude;
  Session.SelectedLocation.Longitude = Longitude;
  ```
- Wrap `GetGps()` and `SelectedSuggestion()` execution in `try-catch-finally` blocks ensuring `IsWorking = false` is always set, preventing infinite loading indicators or ANRs on failures.
- Check return value of `BuildWeatherForecastsAsync` and handle failure gracefully.

#### [MODIFY] [AndroidManifest.xml](file:///F:/source/ThePhotographersForecast/ThePhotographersForecast/Platforms/Android/AndroidManifest.xml)
- Add `<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />`.

---

## Verification Plan

### Automated Tests & Builds
- Run `dotnet build ThePhotographersForecast -f net10.0-android` to verify that the Android target compiles cleanly without errors or warnings.

### Device Verification
- Check Android device via `adb` (`49121FDKD001Z4`).
- Deploy/install the updated debug build to the device.
- Clear logcat, run the app, trigger the GPS / Location weather retrieval.
- Inspect `logcat` to confirm that:
  - `WmoLookup.GetWeather` completes all 168 hours with 0 `InvalidOperationException` errors.
  - `GetWeatherService.BuildWeatherForecastsAsync` completes successfully.
  - Moon phases and NWS services proceed to completion without crashing or causing an ANR.
