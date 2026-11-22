# The Photographer's Forecast

[![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-10%20%2F%2011-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/apps/maui)
[![ONNX Runtime](https://img.shields.io/badge/ONNX%20Runtime-1.26.0-005CED?logo=onnx&logoColor=white)](https://onnxruntime.ai/)
[![PyTorch](https://img.shields.io/badge/PyTorch-Model%20Pipeline-EE4C2C?logo=pytorch&logoColor=white)](https://pytorch.org/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**The Photographer's Forecast** is a cross-platform mobile and desktop application built with **.NET MAUI** and **C#** that predicts optimal outdoor photography and commercial UAS (drone) flight windows. 

By combining **National Weather Service (NWS)** hourly gridpoint weather data, astronomical solar/lunar/galactic ephemeris calculations, dynamic **SkiaSharp** sky visualizations, and an **on-device ONNX neural network**, the app evaluates shooting conditions hour-by-hour with zero cloud inference latency.

---

## Key Features

* **Multi-Window Photographic & Flight Scoring (ONNX v2):** Evaluates a 28-feature meteorological and astronomical tensor in a single batch inference pass (`float32[N, 28] -> float32[N, 7]`) to score:
  * **Drone Flight Viability:** Evaluates sustained wind speed, gust factor, precipitation probability, and visibility envelopes.
  * **Civil Blue Hour:** Identifies crisp atmospheric clarity and scattered high-altitude clouds during civil twilight (`-6°` to `-4°` solar elevation).
  * **Sunrise & Sunset:** Separates low-level stratus clouds (`< 2,000 m`) from high/mid-level cirrus (`> 2,000 m`) capable of catching warm horizon color.
  * **Lunar Rise & Set:** Tracks moonrise/moonset horizon elevation windows alongside dewpoint depression haze proxies.
  * **Galactic Core (Milky Way):** Scores dark-sky visibility windows when solar elevation is below `-12°` to `-18°`, Sagittarius A* altitude is above the coastal light dome, and lunar interference is minimal.
* **NWS Gridpoint Weather Pipeline:** Fetches and parses high-resolution `/gridpoints/{wfo}/{x},{y}` forecast data including cloud ceiling heights, dewpoint depression, wind shear/gusts, and 3-hour thermal lapse deltas.
* **Real-Time Ephemeris Engine:** Computes solar elevation/azimuth, lunar illumination fraction and elevation, and Galactic Center altitude/azimuth for any coordinate and timestamp.
* **Dynamic Sky Visualizations:** Renders real-time twilight and atmospheric sky gradients using **SkiaSharp** and **Skottie**.
* **Local Shoot Logging & Offline Caching:** Persists location profiles, forecast snapshots, and personal shoot ratings locally in **SQLite** for offline field access and future model fine-tuning.

---

## System Architecture

```text
┌─────────────────────────────────────────────────────────────────────────┐
│                        .NET MAUI Client (C# / MVVM)                     │
│                                                                         │
│  ┌────────────────────┐   ┌────────────────────┐   ┌─────────────────┐  │
│  │  NWS Gridpoint API │   │  Ephemeris Engine  │   │  SQLite Store   │  │
│  │  (Hourly Weather)  │   │ (Sun/Moon/Galaxy)  │   │ (Locations/Logs)│  │
│  └─────────┬──────────┘   └─────────┬──────────┘   └────────┬────────┘  │
│            │                        │                       │           │
│            └───────────┬────────────┘                       │           │
│                        ▼                                    │           │
│         ┌─────────────────────────────┐                     │           │
│         │  28-Feature Tensor Builder  │                     │           │
│         └──────────────┬──────────────┘                     │           │
│                        ▼                                    │           │
│         ┌─────────────────────────────┐                     │           │
│         │  Microsoft.ML.OnnxRuntime   │                     │           │
│         │  (photographers_forecast_v2)│                     │           │
│         └──────────────┬──────────────┘                     │           │
│                        ▼                                    ▼           │
│         ┌─────────────────────────────┐   ┌──────────────────────────┐  │
│         │   7-Head Hourly Scorecards  │──▶│  SkiaSharp Sky Renderer  │  │
│         │  (Blue Hr/Sun/Moon/Galaxy)  │   │  & Timeline ViewModels   │  │
│         └─────────────────────────────┘   └──────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────┘


ThePhotographersForecast/
├── src/
│   ├── PhotographersForecast/             # Main .NET MAUI cross-platform application
│   │   ├── Models/                        # Weather, Ephemeris, and WindowScore records
│   │   ├── ViewModels/                    # CommunityToolkit.Mvvm observable ViewModels
│   │   ├── Views/                         # XAML pages and custom SkiaSharp sky canvases
│   │   ├── Services/
│   │   │   ├── Weather/                   # NWS API client & gridpoint feature mapper
│   │   │   ├── Ephemeris/                 # Solar, lunar, and Galactic Center math
│   │   │   └── Inference/                 # ONNX Runtime session & batch scoring service
│   │   └── Resources/
│   │       └── Raw/
│   │           └── photographers_forecast_v2.onnx  # Bundled on-device ONNX model
│   └── PhotographersForecast.Tests/       # Unit tests & ONNX numerical parity checks
├── ml_pipeline/                           # Python PyTorch training & export scripts
│   ├── build_features.py                  # Vectorized 28-feature engineering pipeline
│   ├── train_export_onnx.py               # Multi-head PyTorch training & ONNX export
│   ├── feature_schema_v2.json             # Input/output tensor index contract
│   └── onnx_parity_fixture.json           # Reference vectors for C# parity testing
├── .gitignore
└── README.md


ONNX Model Specification (photographers_forecast_v2.onnx)
To prevent preprocessing drift between Python training and C# inference, Z-score normalization (mean and std computed on the chronological training split) is embedded directly inside the ONNX graph's input layer.

Input Tensor: hourly_features — float32[forecast_hours, 28] (dynamic batch axis supporting 24h, 48h, or 168h NWS windows)

0–13: Temperature, dewpoint, dewpoint depression, relative humidity, total/low/mid/high cloud cover %, visibility, wind speed, wind gust, gust factor, precipitation probability, 3-hour temperature delta.

14–21: Wind direction (
sin/cos), solar elevation & azimuth (
sin/cos), lunar elevation, lunar interference index, Galactic Center altitude.

22–27: Soft astronomical window weights (w_blue_hour, w_sunrise, w_sunset, w_lunar_event, w_galactic, is_daylight).

Output Tensor: window_scores — float32[forecast_hours, 7] (scaled 0.0 to 100.0)

[0] score_drone_flight

[1] score_blue_hour

[2] score_sunrise

[3] score_sunset

[4] score_lunar_rise_set

[5] score_galaxy_rise_set

[6] score_overall_window

Getting Started
Prerequisites
Visual Studio 2026 with the .NET Multi-platform App UI development workload installed

.NET 10 / .NET 11 SDK

Python 3.11+ with torch, onnx, onnxruntime, numpy, and pandas (only required if retraining or modifying the ONNX model)

Building & Running the .NET MAUI App
Clone the repository:

Bash
git clone [https://github.com/Jeremys-Code/ThePhotographersForecast.git](https://github.com/Jeremys-Code/ThePhotographersForecast.git)
cd ThePhotographersForecast
Open the solution (.sln / .slnx) in Visual Studio 2026.

Restore NuGet packages (Microsoft.ML.OnnxRuntime, CommunityToolkit.Mvvm, CommunityToolkit.Maui, SkiaSharp.Views.Maui.Controls, sqlite-net-pcl).

Select your target platform (Windows Machine, Android Emulator, or iOS Simulator) and press F5 to build and run.

Retraining & Exporting the ONNX Model
Navigate to the ML pipeline directory and install dependencies:

Bash
cd ml_pipeline
pip install torch onnx onnxruntime numpy pandas pyarrow
Run feature engineering, training, and ONNX parity verification:

Bash
python train_export_onnx.py
Copy the newly generated photographers_forecast_v2.onnx into src/PhotographersForecast/Resources/Raw/ (ensuring its Build Action is set to MauiAsset) and run the C# unit test suite to verify numerical parity against onnx_parity_fixture.json.

Tech Stack
Application Framework: .NET MAUI (C#)

Architecture & Tooling: MVVM (CommunityToolkit.Mvvm), CommunityToolkit.Maui

On-Device Machine Learning: PyTorch, ONNX (Opset 17), Microsoft.ML.OnnxRuntime

Graphics & Animation: SkiaSharp, Skottie

Local Persistence: SQLite

Data Sources: National Weather Service (NWS) Gridpoint API (api.weather.gov)

License
This project is licensed under the MIT License — see the LICENSE file for details.


---

### Quick Tip Before Committing `README.md`
Adjust the folder paths under **Repository Structure** (e.g., if your `.csproj` sits directly at the root of `F:\source\ThePhotographersForecast` rather than inside a `src/` subfolder) so every path matches your exact solution layout.

<FollowUp label="Want me to generate the C# feature builder and Visual Studio parity unit test to drop straight into your solution?" query="Write the C# 28-feature builder class and the Visual Studio unit test that loads photographers_forecast_v2.onnx and asserts output parity against onnx_parity_fixture.json."/>