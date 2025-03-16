using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Zs.Home.Application.Features.Weather;

public interface IWeatherAnalyzer
{
    Task<string> GetDeviationInfosAsync(IReadOnlyList<DeviceSettings> deviceSettings, CancellationToken ct);
}
