using MediatR;
using Zs.Home.Application.Features.Hardware;

namespace Zs.Home.WebApi.Features.HardwareStatus.GetCurrent;

public sealed class GetCurrentHardwareStatusHandler : IRequestHandler<GetCurrentHardwareStatusRequest, GetCurrentHardwareStatusResponse>
{
    private readonly IHardwareMonitor _hardwareMonitor;

    public GetCurrentHardwareStatusHandler(IHardwareMonitor hardwareMonitor)
    {
        _hardwareMonitor = hardwareMonitor;
    }

    public async Task<GetCurrentHardwareStatusResponse> Handle(GetCurrentHardwareStatusRequest request, CancellationToken cancellationToken)
    {
        var hardwareStatus = await _hardwareMonitor.GetHardwareStatusAsync(cancellationToken);
        return new GetCurrentHardwareStatusResponse(hardwareStatus);
    }
}
