using Zs.Common.Services.Scheduling;

namespace Zs.Home.Bot.Features.Hardware;

public interface IHasJob
{
    public ProgramJob<string> Job { get; }
}