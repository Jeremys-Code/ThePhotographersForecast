# 🎯 SQLite persistence plan for ThePhotographersForecast

This plan converts the app's in-memory Session state into a local SQLite database using Entity Framework Core (EF Core) with Microsoft.EntityFrameworkCore.Sqlite. The database will store all session data (locations, forecasts, events, settings, photo tracks) and act as the authoritative offline store. Schemas will be managed with automatic migrations on startup.

Rationale and findings from the codebase

- MauiProgram.cs registers a Session singleton and several services (IGetEventService, IGetWeatherService, WeatherReporter, ISkyColorEngine, NavigationService) and view models (SelectLocationViewModel, ForecastViewModel, SessionViewModel). (See: ThePhotographersForecast\MauiProgram.cs)
- The codebase contains domain models and data classes relevant to persistence: PhotoDayModel, ForecastEventModel, EventModel, DaySchedule, PhotoLocation, GeocodeResult, grid/time data under Data\. (Search matches found in the workspace.)
- Services that create data: GetWeatherService (weather), GetEventService (events), CurrentLocation (location), and viewmodels like ForecastViewModel and PhotoTimeTrackViewModel produce or transform session data that needs persistent storage.
- I did not find a concrete Session.cs implementation file in the workspace during exploration; MauiProgram registers Session as a singleton but the source for Session could be in another folder or project. The plan assumes Session currently holds aggregated in-memory state for the models above.

Risks / open questions

- Exact shape of the current Session API is unknown; implementation will require locating the Session class or adapting the DI registration to use a database-backed session service.
- Some domain types (e.g., GridHourlyData, CoordinatesToGridData) are large or generated; decide whether to persist raw grids or only summarized forecasts to limit DB size.
- Migration of existing user data (if any) is not addressed; this plan assumes greenfield or migration via an export/import strategy if needed.

Design overview (entities and relationships)

- Entities to create (EF Core models):
  - Location: id, name, latitude, longitude, geocode metadata, timezone
  - Forecast: id, locationId (FK), generatedAt, source, periodStart, periodEnd, data blob or structured fields (temperature, sky, wind, precipitation), summary fields for UI
  - ForecastEvent: id, forecastId (FK), eventType, severity, startTime, endTime, details
  - PhotoDay / PhotoDayModel: id, locationId (FK), date, goldenHourStart, goldenHourEnd, blueHourStart, blueHourEnd, sunrise/sunset times
  - PhotoTimeTrack (session tracks): id, locationId (FK), timestamp, cameraSettings (json), note, linkedEventId (FK nullable)
  - DaySchedule / EventModel: id, userDate, title, description, locationId (FK nullable), relatedForecastId (FK nullable)
  - Settings: key, value (store user preferences like units, default location id)
  - StoredFiles / Assets (optional): id, path, metadata

- Relationships:
  - Location 1..* Forecast
  - Forecast 1..* ForecastEvent
  - Location 1..* PhotoDay
  - PhotoDay 1..* PhotoTimeTrack
  - Settings is key/value global; optionally separate UserProfile table.

Technical decisions (based on answers provided)

- EF Core with Microsoft.EntityFrameworkCore.Sqlite will be used.
- Database file location: use FileSystem.AppDataDirectory (MAUI) with a fixed filename (photographersforecast.db).
- Use a single EF Core DbContext (e.g., AppDbContext) registered as a scoped service; provide a thin repository/service layer (DataAccess) registered as singleton or scoped for app components.
- Auto-migrate on startup using context.Database.Migrate() to apply EF Core migrations automatically.
- Serialize complex or rarely-queried fields as JSON columns (string) using System.Text.Json; configure EF Core value conversions for these properties.
- Provide an IAppStorage or ISessionStore interface to replace the in-memory Session — implementation will use EF Core to read/write state. Register the implementation and replace existing Session registration in MauiProgram.

Implementation steps

1. Add EF Core packages — modify project file and restore:
   - Add Microsoft.EntityFrameworkCore
   - Add Microsoft.EntityFrameworkCore.Sqlite
   - Add Microsoft.EntityFrameworkCore.Design (for migrations, dev only)

2. Create Entities and AppDbContext:
   - Create Models/Entities folder with classes: Location, Forecast, ForecastEvent, PhotoDay, PhotoTimeTrack, DaySchedule, Setting.
   - Create Data/AppDbContext : DbContext with DbSet<T> for each entity and model configuration (Fluent API) including indices and FK constraints.
   - Configure value conversions for JSON properties (e.g., detailed forecast payload).

3. Create repository/service layer:
   - Create Services/DataAccess (or Database/DataAccess) exposing methods to Get/Save/Delete for each aggregate type and to run queries the app needs (e.g., get latest forecast for location, get photo day by date/location).
   - Implement caching within DataAccess where necessary for performance.

4. Replace Session with database-backed store:
   - Locate Session class; if absent, add an ISessionStore interface and an EfSessionStore implementation that maps prior Session responsibilities to DB operations.
   - In MauiProgram.cs, replace or register EfSessionStore and adjust DI registrations. Keep SessionViewModel DI to depend on ISessionStore or DataAccess.

5. Startup migration and initialization:
   - Register AppDbContext in MauiProgram: builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=" + dbPath));
   - After building the app (or in App.OnStart), resolve AppDbContext and call context.Database.Migrate(); optionally seed default data.

6. Update services and viewmodels to use DataAccess:
   - Update GetWeatherService/GetEventService/WeatherReporter to persist fetched results to DB.
   - Update ForecastViewModel, SelectLocationViewModel, PhotoTimeTrackViewModel to read/write from DataAccess instead of Session fields.

7. Tests and verification:
   - Add unit tests (if test infra exists) for AppDbContext mapping and DataAccess methods.
   - Manual QA: run scenarios (create location, fetch forecast, save photo track, view offline data).

8. Optional improvements (post-MVP):
   - Add encryption for DB (SQLCipher) if needed.
   - Add sync mechanism for cloud backup.
   - Add background pruning policy for old forecast cache rows.

Files to add/modify (suggested list)

- Add: Data/AppDbContext.cs
- Add: Models/Location.cs, Models/Forecast.cs, Models/ForecastEvent.cs, Models/PhotoDay.cs, Models/PhotoTimeTrack.cs, Models/DaySchedule.cs, Models/Setting.cs
- Add: Services/DataAccess.cs (implements IDataAccess interface)
- Modify: ThePhotographersForecast\MauiProgram.cs to register DbContext and DataAccess and replace Session registration
- Modify: Services/GetWeatherService.cs and Services/GetEventService.cs to persist fetched data
- Modify: ViewModels (ForecastViewModel, SelectLocationViewModel, PhotoTimeTrackViewModel, SessionViewModel) to use IDataAccess/ISessionStore

**Last Updated**: 2026-07-16 04:20:01

## 📝 Plan Steps
-  **Add EF Core NuGet packages to the MAUI project — Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.Sqlite, Microsoft.EntityFrameworkCore.Design.**
-  **Create entity classes for Location, Forecast, ForecastEvent, PhotoDay, PhotoTimeTrack, DaySchedule, Setting.**
-  **Implement AppDbContext with DbSets and Fluent API configuration and JSON value conversions.**
-  **Add DataAccess service with CRUD + query methods; register it in DI.**
-  **Implement EfSessionStore (or adapt Session) that maps Session responsibilities to DB operations.**
-  **Update MauiProgram.cs to register DbContext and call migrations on startup.**
-  **Update services and viewmodels to persist and load data from DataAccess instead of in-memory Session.**
-  **Test manually and add unit tests for key repository methods.**

