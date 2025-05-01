using MediatR;
using Microsoft.Extensions.Options;
using Zs.Home.Application.Features.Hardware;

namespace Zs.Home.WebApi.Features.HardwareStatus.GetStatusLimits;

public sealed class GetHardwareStatusLimitsHandler : IRequestHandler<GetHardwareStatusLimitsRequest, GetHardwareStatusLimitsResponse>
{
    private readonly HardwareMonitorSettings _settings;

    public GetHardwareStatusLimitsHandler(IOptions<HardwareMonitorSettings> options)
    {
        _settings = options.Value;
    }

    public Task<GetHardwareStatusLimitsResponse> Handle(GetHardwareStatusLimitsRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new GetHardwareStatusLimitsResponse(_settings.Limits));
    }
}
