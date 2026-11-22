"""
Training and ONNX export script for The Photographer's Forecast.
Generates synthetic meteorological + astronomical training samples across 21 features,
trains a multi-head PyTorch MLP, and exports photographers_forecast.onnx with embedded
feature normalization and 8 output heads:
  - hourly_drone_flight_score
  - hourly_blue_hour_score
  - hourly_sunrise_score
  - hourly_sunset_score
  - hourly_lunar_event_score
  - hourly_galaxy_score
  - hourly_overall_score
  - hourly_condition_probs
"""

from pathlib import Path
import numpy as np
import torch
import torch.nn as nn
import onnx

SEED = 42
rng = np.random.default_rng(SEED)
torch.manual_seed(SEED)

FEATURE_COUNT = 21
NUM_SAMPLES = 24000


def generate_dataset(n: int):
    temp_c = rng.uniform(-10.0, 38.0, n).astype(np.float32)
    humidity = rng.uniform(15.0, 100.0, n).astype(np.float32)
    dew_spread = np.clip((100.0 - humidity) * 0.25 + rng.normal(0, 1.5, n), 0.0, 28.0).astype(np.float32)
    dewpoint_c = (temp_c - dew_spread).astype(np.float32)
    wind10 = rng.exponential(14.0, n).clip(0.0, 75.0).astype(np.float32)
    gust = (wind10 * rng.uniform(1.05, 1.65, n)).clip(0.0, 110.0).astype(np.float32)
    apparent_c = (temp_c - 0.1 * wind10 + 0.04 * humidity).astype(np.float32)

    wind_deg = rng.uniform(0.0, 360.0, n).astype(np.float32)
    wind_rad = np.deg2rad(wind_deg)
    sin_w = np.sin(wind_rad).astype(np.float32)
    cos_w = np.cos(wind_rad).astype(np.float32)

    cloud_low = rng.uniform(0.0, 100.0, n).astype(np.float32)
    cloud_mid = rng.uniform(0.0, 100.0, n).astype(np.float32)
    cloud_high = rng.uniform(0.0, 100.0, n).astype(np.float32)
    sky_cover = np.clip(0.5 * cloud_low + 0.3 * cloud_mid + 0.2 * cloud_high + rng.normal(0, 5, n), 0.0, 100.0).astype(np.float32)

    precip_prob = np.clip(0.6 * sky_cover + 0.3 * humidity - 25.0 + rng.normal(0, 15, n), 0.0, 100.0).astype(np.float32)
    precip_mm = np.where(precip_prob > 45.0, rng.exponential(2.0, n) * (precip_prob / 100.0), 0.0).clip(0.0, 30.0).astype(np.float32)

    solar_elev = rng.uniform(-60.0, 75.0, n).astype(np.float32)
    daylight_flag = np.where(solar_elev >= 0.0, 1.0, np.where(solar_elev >= -6.0, 0.5, 0.0)).astype(np.float32)

    visibility_km = np.clip(24.0 - 0.12 * humidity - 1.5 * precip_mm - 0.05 * cloud_low + rng.normal(0, 2.0, n), 0.3, 30.0).astype(np.float32)
    ceiling_m = np.where(cloud_low > 60.0, rng.uniform(150.0, 1800.0, n), rng.uniform(2000.0, 12000.0, n)).astype(np.float32)
    wind120 = (wind10 * rng.uniform(1.15, 1.75, n)).clip(0.0, 120.0).astype(np.float32)

    moon_elev = rng.uniform(-50.0, 70.0, n).astype(np.float32)
    moon_illum = rng.uniform(0.0, 1.0, n).astype(np.float32)

    X = np.stack([
        temp_c, apparent_c, dewpoint_c, humidity,
        wind10, gust, sin_w, cos_w,
        sky_cover, precip_prob, solar_elev, daylight_flag,
        ceiling_m, cloud_low, cloud_mid, cloud_high,
        visibility_km, wind120, precip_mm, moon_elev, moon_illum
    ], axis=1).astype(np.float32)

    # 1. Drone Flight Score (0-100): penalized by 120m wind, gusts, precip, low visibility, low ceiling
    drone_score = (
        100.0
        - np.maximum(0.0, wind120 - 18.0) * 1.35
        - np.maximum(0.0, gust - 25.0) * 1.15
        - precip_prob * 0.45
        - precip_mm * 8.0
        - np.maximum(0.0, 5.0 - visibility_km) * 10.0
        - np.where(ceiling_m < 250.0, 20.0, 0.0)
    )
    drone_score = np.clip(drone_score, 0.0, 100.0).astype(np.float32)

    # 2. Blue Hour Score (0-100): peaks around solar elevation -6 to -3 deg with high/mid clouds & good visibility
    blue_window = np.exp(-0.5 * ((solar_elev + 4.5) / 2.2) ** 2)
    cloud_drama = np.clip(1.0 - np.abs(cloud_high - 45.0) / 65.0, 0.2, 1.0) * (1.0 - 0.55 * (cloud_low / 100.0))
    wx_clear_factor = np.clip((1.0 - precip_prob / 120.0) * np.clip(visibility_km / 12.0, 0.2, 1.0), 0.0, 1.0)
    blue_score = np.clip(100.0 * blue_window * (0.55 + 0.45 * cloud_drama) * wx_clear_factor, 0.0, 100.0).astype(np.float32)

    # 3. Sunrise / Sunset Score (0-100): peaks around solar elevation -1 to +6 deg with textured mid/high clouds
    golden_window = np.exp(-0.5 * ((solar_elev - 2.0) / 4.5) ** 2)
    golden_clouds = np.clip(0.4 + 0.6 * np.exp(-0.5 * ((cloud_mid + cloud_high - 75.0) / 50.0) ** 2), 0.2, 1.0)
    low_block = 1.0 - 0.7 * (cloud_low / 100.0)
    sunrise_score = np.clip(100.0 * golden_window * golden_clouds * low_block * wx_clear_factor, 0.0, 100.0).astype(np.float32)
    # Sunset gets slight humidity/aerosol warmth bonus when dew spread is moderate
    sunset_warmth = np.clip(1.0 + 0.1 * np.exp(-0.5 * ((humidity - 55.0) / 20.0) ** 2), 0.9, 1.1)
    sunset_score = np.clip(sunrise_score * sunset_warmth, 0.0, 100.0).astype(np.float32)

    # 4. Lunar Event Score (0-100): requires moon above horizon, high illumination, clear sky, good visibility
    moon_vis_window = np.where(moon_elev > 0.0, np.clip(moon_elev / 20.0, 0.2, 1.0), 0.0)
    clear_sky = (1.0 - sky_cover / 105.0)
    lunar_score = np.clip(100.0 * moon_vis_window * (0.35 + 0.65 * moon_illum) * clear_sky * wx_clear_factor, 0.0, 100.0).astype(np.float32)

    # 5. Galaxy / Astro Score (0-100): requires dark sky (solar_elev < -12), low moon light, very clear sky
    astro_dark = np.clip((-solar_elev - 10.0) / 8.0, 0.0, 1.0)
    dark_moon = 1.0 - 0.75 * moon_illum * np.where(moon_elev > 0.0, 1.0, 0.0)
    galaxy_score = np.clip(100.0 * astro_dark * dark_moon * (1.0 - sky_cover / 100.0) ** 1.5 * wx_clear_factor, 0.0, 100.0).astype(np.float32)

    # 6. Overall Photography Score (0-100): best available opportunity + general outdoor crispness
    general_day = np.clip(
        (78.0 - 0.35 * np.abs(sky_cover - 35.0) - 0.45 * precip_prob - 0.3 * wind10 + np.clip(visibility_km, 0, 20) * 0.8)
        * np.where(solar_elev >= -6.0, 1.0, 0.45),
        0.0, 100.0
    )
    overall_score = np.clip(
        0.50 * np.maximum.reduce([blue_score, sunrise_score, sunset_score, lunar_score, galaxy_score, general_day])
        + 0.35 * general_day
        + 0.15 * drone_score,
        0.0, 100.0
    ).astype(np.float32)

    # 7. Condition class (0..7 matching PhotoWeatherCondition enum)
    cond = np.zeros(n, dtype=np.int64)
    for i in range(n):
        if precip_prob[i] > 65 and gust[i] > 45:
            cond[i] = 7  # SevereStormy
        elif precip_prob[i] > 50 or precip_mm[i] > 0.5:
            cond[i] = 6  # RainShowers
        elif visibility_km[i] < 4.0 or (dew_spread[i] < 2.2 and humidity[i] > 85):
            cond[i] = 5  # MistyFogRisk
        elif wind10[i] > 32 or gust[i] > 48:
            cond[i] = 4  # WindyGusty
        elif sky_cover[i] > 78:
            cond[i] = 3  # OvercastFlatLight
        elif -6.0 <= solar_elev[i] <= 8.0 and sky_cover[i] < 70:
            cond[i] = 1  # GoldenDramaticLight
        elif sky_cover[i] >= 30:
            cond[i] = 2  # PartlyCloudy
        else:
            cond[i] = 0  # ClearCrisp

    return X, drone_score, blue_score, sunrise_score, sunset_score, lunar_score, galaxy_score, overall_score, cond


class PhotographersForecastNet(nn.Module):
    def __init__(self, mean: np.ndarray, scale: np.ndarray):
        super().__init__()
        self.register_buffer("mean", torch.tensor(mean, dtype=torch.float32))
        self.register_buffer("scale", torch.tensor(scale, dtype=torch.float32))

        self.backbone = nn.Sequential(
            nn.Linear(FEATURE_COUNT, 128),
            nn.ReLU(),
            nn.Linear(128, 96),
            nn.ReLU(),
            nn.Linear(96, 64),
            nn.ReLU(),
        )
        self.drone_head = nn.Linear(64, 1)
        self.blue_head = nn.Linear(64, 1)
        self.sunrise_head = nn.Linear(64, 1)
        self.sunset_head = nn.Linear(64, 1)
        self.lunar_head = nn.Linear(64, 1)
        self.galaxy_head = nn.Linear(64, 1)
        self.overall_head = nn.Linear(64, 1)
        self.cond_head = nn.Linear(64, 8)

    def forward(self, x):
        z = (x - self.mean) / self.scale
        h = self.backbone(z)
        return (
            torch.clamp(self.drone_head(h), 0.0, 100.0),
            torch.clamp(self.blue_head(h), 0.0, 100.0),
            torch.clamp(self.sunrise_head(h), 0.0, 100.0),
            torch.clamp(self.sunset_head(h), 0.0, 100.0),
            torch.clamp(self.lunar_head(h), 0.0, 100.0),
            torch.clamp(self.galaxy_head(h), 0.0, 100.0),
            torch.clamp(self.overall_head(h), 0.0, 100.0),
            torch.softmax(self.cond_head(h), dim=-1),
        )


def train_and_export():
    X, drone, blue, sunrise, sunset, lunar, galaxy, overall, cond = generate_dataset(NUM_SAMPLES)
    mean = X.mean(axis=0)
    scale = np.where(X.std(axis=0) < 1e-5, 1.0, X.std(axis=0)).astype(np.float32)

    model = PhotographersForecastNet(mean, scale)
    optimizer = torch.optim.Adam(model.parameters(), lr=3e-3)
    mse = nn.MSELoss()
    ce = nn.CrossEntropyLoss()

    ds = torch.utils.data.TensorDataset(
        torch.tensor(X),
        torch.tensor(drone).unsqueeze(1),
        torch.tensor(blue).unsqueeze(1),
        torch.tensor(sunrise).unsqueeze(1),
        torch.tensor(sunset).unsqueeze(1),
        torch.tensor(lunar).unsqueeze(1),
        torch.tensor(galaxy).unsqueeze(1),
        torch.tensor(overall).unsqueeze(1),
        torch.tensor(cond),
    )
    loader = torch.utils.data.DataLoader(ds, batch_size=512, shuffle=True)

    model.train()
    for epoch in range(25):
        total_loss = 0.0
        for bx, bd, bb, bsr, bss, bl, bg, bo, bc in loader:
            optimizer.zero_grad()
            z = (bx - model.mean) / model.scale
            h = model.backbone(z)
            loss = (
                mse(model.drone_head(h), bd)
                + mse(model.blue_head(h), bb)
                + mse(model.sunrise_head(h), bsr)
                + mse(model.sunset_head(h), bss)
                + mse(model.lunar_head(h), bl)
                + mse(model.galaxy_head(h), bg)
                + mse(model.overall_head(h), bo)
                + 50.0 * ce(model.cond_head(h), bc)
            )
            loss.backward()
            optimizer.step()
            total_loss += loss.item()
        if (epoch + 1) % 5 == 0:
            print(f"Epoch {epoch + 1}/25 - Loss: {total_loss / len(loader):.3f}")

    model.eval()
    output_path = Path(__file__).resolve().parent.parent / "ThePhotographersForecast" / "Resources" / "Raw" / "photographers_forecast.onnx"
    output_path.parent.mkdir(parents=True, exist_ok=True)

    dummy_input = torch.randn(1, FEATURE_COUNT, dtype=torch.float32)
    output_names = [
        "hourly_drone_flight_score",
        "hourly_blue_hour_score",
        "hourly_sunrise_score",
        "hourly_sunset_score",
        "hourly_lunar_event_score",
        "hourly_galaxy_score",
        "hourly_overall_score",
        "hourly_condition_probs",
    ]
    dynamic_axes = {"nws_hourly_features": {0: "batch"}}
    for name in output_names:
        dynamic_axes[name] = {0: "batch"}

    torch.onnx.export(
        model,
        dummy_input,
        str(output_path),
        input_names=["nws_hourly_features"],
        output_names=output_names,
        dynamic_axes=dynamic_axes,
        opset_version=17,
        dynamo=False,
    )

    onnx_model = onnx.load(str(output_path))
    onnx.checker.check_model(onnx_model)
    print(f"Exported and verified ONNX model at: {output_path} ({output_path.stat().st_size} bytes)")


if __name__ == "__main__":
    train_and_export()
