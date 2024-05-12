using Zs.Common.Services.Scheduling;
using Zs.Home.Application.Features.Hardware;

namespace Zs.Home.Application.Features.Seq;

public interface ISeqEventsInformer : IHasCurrentState
{
    ProgramJob<string> DayEventsInformerJob { get; }
    ProgramJob<string> NightEventsInformerJob { get; }
}
