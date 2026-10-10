namespace ThePhotographersForecast.Models
{
    public class NwsHeatriskDetail
    {
        public DateTime Date { get; set; }
        public int Heatrisk { get; set; }
        public string HeatriskDescription => GetHeatRiskDescription();
        public Color HeatRiskColor => GetHeatRiskColor();
        private Color GetHeatRiskColor()
        {
            switch (this.Heatrisk)
            {
                case 0:
                    return Color.FromRgb(0, 255, 0);
                case 1:
                    return Color.FromRgb(255, 255, 0);
                case 2:
                    return Color.FromRgb(255, 165, 0);
                case 3:
                    return Color.FromRgb(255, 0, 0);
                case 4:
                    return Color.FromRgb(255, 0, 255);
                default:
                    return Color.FromRgb(255, 255, 255);
            }

        }
        private string GetHeatRiskDescription()
        {
            switch (this.Heatrisk)
            {
                case 0:
                    return "There is a category 0 (Green) ForecastHeatRisk. Little to no risk from expected heat.";
                case 1:
                    return "There is a category 1 (Yellow) ForecastHeatRisk. Minor - Primarily affects those who are extremely sensitive to heat and without cooling/hydration.";
                case 2:
                    return "There is a category 2 (Orange) ForecastHeatRisk. Moderate - Affects those who are sensitive to heat, especially those without cooling/hydration, and some health systems and industries.";
                case 3:
                    return "There is a category 3 (Red) ForecastHeatRisk. Major - Affects anyone without cooling/hydration as well as health systems and industries.";
                case 4:
                    return "There is a category 4 (Magenta/Purple) ForecastHeatRisk. Extreme - Rare and/or long-duration extreme heat with no overnight relief affecting anyone without cooling/hydration as well as health systems, industries, and infrastructure.";
                default:
                    return "There is no ForecastHeatRisk category issued today.";
            }
        }
    }
}
