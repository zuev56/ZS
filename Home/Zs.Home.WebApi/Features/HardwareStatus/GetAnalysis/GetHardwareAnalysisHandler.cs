using MediatR;
using Microsoft.Extensions.Options;
using Zs.Home.Application.Features.Hardware;

namespace Zs.Home.WebApi.Features.HardwareStatus.GetAnalysis;

public sealed class GetHardwareAnalysisHandler : IRequestHandler<GetHardwareAnalysisRequest, GetHardwareAnalysisResponse>
{
    private readonly IHardwareMonitor _hardwareMonitor;
    private readonly Limits _limits;

    public GetHardwareAnalysisHandler(IHardwareMonitor hardwareMonitor, IOptions<HardwareMonitorSettings> options)
    {
        _hardwareMonitor = hardwareMonitor;
        _limits = options.Value.Limits;
    }

    public async Task<GetHardwareAnalysisResponse> Handle(GetHardwareAnalysisRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("РЕШИЛ ПОКА НЕ ДЕЛАТЬ, Т.К, АНАЛИЗ ТУТ ОЧЕНЬ ПРОСТОЙ, " +
                                          "А РЕЗУЛЬТАТЫ ВЫВОДИТЬ ПРИДЁТСЯ ВЕЗДЕ ПО-СВОЕМУ. " +
                                          "ЗАМОРАЧИВАТЬСЯ С ОТДЕЛЬНОЙ МОДЕЛЬЮ НЕТ СМЫСЛА");

        // var hardwareStatus = await _hardwareMonitor.GetHardwareStatusAsync(cancellationToken);
        //
        // var dictionary = new Dictionary<string, bool>();
        //
        // if (hardwareStatus.Cpu15MinUsagePercent >= _limits.CpuUsagePercent)
        //     throw new NotImplementedException();
        //
        // if (hardwareStatus.CpuTemperatureC >= _limits.CpuTemperatureC)
        //     throw new NotImplementedException();
        //
        // if (hardwareStatus.MemoryUsagePercent >= _limits.MemoryUsagePercent)
        //     throw new NotImplementedException();
        //
        // if (hardwareStatus.StorageUsagePercent >= _limits.StorageUsagePercent)
        //     throw new NotImplementedException();
        //
        // if (hardwareStatus.StorageTemperatureC >= _limits.StorageTemperatureC)
        //     throw new NotImplementedException();
        //
        // return new GetHardwareAnalysisResponse(hardwareStatus);
    }
}
