using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Zs.Home.Application.Features.Weather;

public interface IWeatherAnalyzer
{
    Task<IReadOnlyList<EspMeteoAnalysisResult>> AnalyseAsync(IReadOnlyList<DeviceSettings> deviceSettings, CancellationToken ct);
}
