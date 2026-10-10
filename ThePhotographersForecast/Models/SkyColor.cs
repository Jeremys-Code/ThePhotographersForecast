namespace ThePhotographersForecast.Models
{

    //    // ABOUT THIS CLASS:
    //    1. PM2.5 (Fine particulate matter)
    //This is the single most important air‑quality variable for sky color.
    //Why:
    //PM2.5 particles are the perfect size for Mie scattering
    //They mute reds/oranges
    //They flatten gradients
    //They produce “milky” sunsets
    //They reduce twilight saturation
    //This replaces Visibility as your primary aerosol proxy.
    //2. PM10 (Coarse particulate matter)
    //Why:
    //Larger particles → stronger forward scattering
    //Produces yellowish tint
    //Reduces contrast
    //Makes twilight bands less defined\
    //PM10 helps distinguish dust‑driven haze from moisture‑driven haze.
    //3. Ozone (O₃)
    //Why:
    //High ozone → increased UV absorption
    //Slightly shifts blue intensity
    //Can deepen or flatten the blue hour depending on concentration
    //Not as strong as PM2.5, but meaningful.
    //4. Nitrogen Dioxide (NO₂)
    //Why:
    //NO₂ absorbs blue wavelengths
    //High NO₂ → brownish/yellowish cast
    //Reduces blue hour vibrance
    //This is especially important in urban photography(Boston included).
    //5. Sulfur Dioxide(SO₂)
    //Why:
    //SO₂ forms sulfate aerosols
    //Increases Mie scattering
    //Produces pale, washed‑out sunsets
    //Reduces color purity
    //6.Carbon Monoxide(CO)
    //Why:
    //CO itself doesn’t scatter light
    //But it correlates strongly with combustion aerosols
    //Helps refine your aerosol model
    //8. Surface Pressure(from weather API)
    //Why:
    //Controls airmass thickness
    //Higher pressure → stronger Rayleigh scattering → deeper blues
    //Lower pressure → weaker Rayleigh → flatter sky
    //This remains part of your model even with the air‑quality API.
    public class SkyColor
    {
        public DateTime Time { get; set; }
        public double? Pm10 { get; set; }
        public double? Pm25 { get; set; }
        public double? CarbonMonoxide { get; set; } = 0;

        public double? NitrogenDioxide { get; set; }
        public double? SulphurDioxide { get; set; }
        public double? Ozone { get; set; }
        public double? Dust { get; set; }
        public double? UvIndex { get; set; }
        public double? UvIndexClearSky { get; set; }

        /// <summary>
        /// Direct Solar Radiation(or Direct Normal Irradiance - DNI) : 
        /// This is the direct light traveling in a straight line from the sun to the earth.For a photographer, 
        /// this translates to harsh directional shadows and the intense, warm beams of golden hour.
        /// Diffuse Solar Radiation (DHI): This is the sunlight that has been scattered by molecules, aerosols, 
        /// and clouds in the atmosphere. This is the most crucial variable for your sky color logic, as it 
        /// dictates the ambient light and the glowing reds, pinks, and purples in the sky during sunrise 
        /// and sunset.Shortwave Solar Radiation (GHI): This is the total global horizontal irradiation (essentially
        /// Direct + Diffuse). It's helpful as a baseline for total ambient brightness.
        /// </summary>
        public double SurfacePressure { get; set; }
        public double DiffuseRadiation { get; set; }
        public double DirectNormalIrradiance { get; set; }
        public double ShortwaveRadiation { get; set; }
        public double CloudCoverPercent { get; set; }
        public double SunAltitudeDeg { get; set; }
        public double SunAzimuthDeg { get; set; }
        public DateTime Sunrise { get; set; }
        public DateTime Sunset { get; set; }
        public DateTime SolarNoon { get; set; }

    }
}
