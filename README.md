The Photographer’s Forecast
A precision‑engineered MAUI application for photographers who chase perfect light.
The Photographer’s Forecast is a cross‑platform .NET MAUI application designed for photographers who rely on accurate weather, astronomical timing, and sky‑color prediction to plan their shoots. Built with a modern, refactored architecture, the app provides fast, reliable, and visually rich forecasting tailored specifically for photography.

This README explains the app’s purpose, architecture, features, and upcoming enhancements.

🌅 Purpose
The Photographer’s Forecast helps photographers answer the most important question:

“When will the sky look its best?”

To do this, the app combines:

authoritative weather data

astronomical calculations

twilight modeling

sky‑color prediction

photo‑specific scheduling

intuitive navigation

It is built for aerial photographers, landscape shooters, and anyone who relies on precise environmental conditions.

📱 Core Features
1. Location‑Aware Forecasting
Select your shooting location and instantly receive:

detailed weather conditions

sky coverage

visibility

wind profiles

precipitation type + intensity

temperature + feels‑like

hazard indicators

All location data is stored in a singleton SessionStore, ensuring consistency across the app.

2. Twilight & Astronomical Timing
The app computes:

civil, nautical, and astronomical twilight

sunrise / sunset

moonrise / moonset

golden hour

blue hour

solar elevation

lunar illumination

These values feed directly into the sky‑color engine.

3. Sky‑Color Engine (v1)
The sky‑color engine models:

Rayleigh scattering

aerosol optical depth

cloud deck height

horizon clarity

humidity + particulates

wind direction

temperature gradients

It produces color‑phase predictions for:

pre‑sunrise glow

golden hour warmth

blue hour saturation

post‑sunset gradients

This engine is being actively refined (see “Coming Soon”).

4. Photo‑Time Tracking
A dedicated view shows:

optimal shooting windows

transitions between sky phases

expected color intensity

weather‑dependent adjustments

This helps photographers plan exact takeoff times for drones or arrival times for ground shoots.

5. Clean, Modern UI
Built with:

Material 3 on Android

Fluent icons

custom fonts

optimized XAML via SourceGen

fast navigation via Shell routes

The UI is intentionally minimal, fast, and photography‑focused.

🧩 Architecture Overview
Dependency Injection
The app uses a clean DI structure:

Singleton Services
ISessionStore → shared app state

IWeatherService → API + payload handling

WeatherEngine → transforms raw weather into photo‑ready data

AstronomyEngine → sun/moon/twilight calculations

ImageCacheService → icon caching

ServiceHelper → shared utilities

Singletons ensure consistency across navigation and prevent recomputation.

Transient ViewModels
Each page receives a fresh ViewModel instance:

ForecastViewModel

SelectLocationViewModel

SettingsViewModel

DayViewModel

EventsViewModel

PhotoTimeTrackViewModel

This prevents stale UI state and ensures accurate recalculation.

Shell Routing
Pages are registered via:

csharp
Routing.RegisterRoute(nameof(ForecastPage), typeof(ForecastPage));
Routing.RegisterRoute(nameof(SelectLocationPage), typeof(SelectLocationPage));
Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
Views are not registered in DI — Shell instantiates them.

🗺️ Coming Soon
1. Map Tile Background (Major Feature)
A fully integrated map background using:

OpenStreetMap tiles

Mapsui 8

custom tile caching

NetTopologySuite for projections

optimized Android compatibility

This will allow:

location selection directly on the map

visual context for forecasts

sky‑color overlays

horizon‑aware predictions

This is a major UI upgrade.

2. Sky‑Color Engine v2 (Fine‑Tuning)
The next version will include:

improved aerosol modeling

horizon obstruction detection

humidity‑based color dampening

inversion layer detection

sea‑breeze color shifts (Boston‑specific)

storm‑edge color bloom prediction

enhanced blue‑hour saturation modeling

This will make predictions significantly more accurate.

3. Photo‑Event Timeline
A new timeline view showing:

color transitions

weather changes

astronomical events

hazard windows

recommended shooting times

This will replace older timeline pages removed during the refactor.

4. Cached Forecast History
The app will store:

previous forecasts

previous sky‑color predictions

previous photo‑time windows

This allows comparison across days and seasons.

5. Improved Asset Pipeline
Fixes for:

image caching

font aliasing

splash/icon refresh

cross‑platform asset consistency

This ensures updated icons and fonts always display correctly.

🧭 Project Structure
Code
ThePhotographersForecast/
│
├── Views/
│   ├── Forecast.xaml
│   ├── SelectLocation.xaml
│   ├── Settings.xaml
│   └── (more coming soon)
│
├── ViewModels/
│   ├── ForecastViewModel.cs
│   ├── SelectLocationViewModel.cs
│   ├── SettingsViewModel.cs
│   └── (secondary viewmodels)
│
├── Services/
│   ├── WeatherEngine.cs
│   ├── AstronomyEngine.cs
│   ├── IWeatherService.cs
│   ├── ImageCacheService.cs
│   └── ServiceHelper.cs
│
├── Shared/
│   └── ISessionStore.cs
│
├── Resources/
│   ├── Images/
│   ├── Fonts/
│   └── Raw/
│
└── AppShell.xaml
📸 Why This App Exists
The Photographer’s Forecast is built for photographers who care about:

color

light

timing

atmosphere

precision

It’s not a generic weather app — it’s a photography tool, engineered with the same care you put into your aerial work.