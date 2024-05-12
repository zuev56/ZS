using Zs.Common.Services.Scheduling;

namespace Zs.Home.Application.Features.Hardware;

public interface IHasJob
{
    public ProgramJob<string> Job { get; }
}