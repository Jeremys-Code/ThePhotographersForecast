namespace ThePhotographersForecast.Models.ObserverCalc.Dtos
{
    /// <summary>
    /// The eight principal and intermediate phases of the Moon
    /// </summary>
    public enum MoonPhase
    {
        /// <summary>
        /// Completely in Sun's shadow
        /// </summary>
        NewMoon,

        /// <summary>
        /// Disc between 0.1% and 49.9% lit, and increasing
        /// </summary>
        WaxingCrescent,

        /// <summary>
        /// Disc 50% lit
        /// </summary>
        FirstQuarter,

        /// <summary>
        /// Disc between 50.1% and 99.9% lit, and increasing
        /// </summary>
        WaxingGibbous,

        /// <summary>
        /// 100% illuminated
        /// </summary>
        FullMoon,

        /// <summary>
        /// Disc between 99.9% and 50.1% lit, and decreasing
        /// </summary>
        WaningGibbous,

        /// <summary>
        /// 50% lit
        /// </summary>
        LastQuarter,

        /// <summary>
        /// Disc between 49.9% and 0.1% lit, and decreasing
        /// </summary>
        WaningCrescent
    }

    /// <summary>
    /// Maps moon phases
    /// </summary>
    public static class MoonPhaseMapper
    {
        /// <summary>
        /// Map moon phase based on how far the moon is through the lunar cycle
        /// </summary>
        /// <param name="progressThroughCycle">percentage value in the range 0 to 1, defining how far the moon is through the lunar cycle</param>
        /// <returns><see cref="MoonPhase"/></returns>
        public static MoonPhase FromProgressThroughCycle(double progressThroughCycle)
        {
            if (progressThroughCycle < 0.2)
            {
                return MoonPhase.NewMoon;
            }

            if (progressThroughCycle >= 0.2 && progressThroughCycle <= 0.24)
            {
                return MoonPhase.WaxingCrescent;
            }

            if (progressThroughCycle > 0.24 && progressThroughCycle < 0.26)
            {
                return MoonPhase.FirstQuarter;
            }

            if (progressThroughCycle >= 0.26 && progressThroughCycle <= 0.49)
            {
                return MoonPhase.WaxingGibbous;
            }

            if (progressThroughCycle > 0.49 && progressThroughCycle < 0.51)
            {
                return MoonPhase.FullMoon;
            }

            if (progressThroughCycle >= 0.51 && progressThroughCycle <= 0.74)
            {
                return MoonPhase.WaningGibbous;
            }

            if (progressThroughCycle > 0.74 && progressThroughCycle < 0.76)
            {
                return MoonPhase.LastQuarter;
            }

            return MoonPhase.WaningCrescent;
        }
    }
}