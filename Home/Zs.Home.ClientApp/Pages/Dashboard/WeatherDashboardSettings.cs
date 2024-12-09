using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Zs.Home.ClientApp.Pages.Dashboard;

public sealed class WeatherDashboardSettings
{
    public const string SectionName = "WeatherDashboard";

    [Required]
    public IReadOnlyList<Parameter> Parameters { get; set; } = null!;

    public sealed class Parameter
    {
        [Required]
        public short PlaceId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public double? LoLo { get; set; }
        public double? Lo { get; set; }
        public double? Hi { get; set; }
        public double? HiHi { get; set; }
    }
}
