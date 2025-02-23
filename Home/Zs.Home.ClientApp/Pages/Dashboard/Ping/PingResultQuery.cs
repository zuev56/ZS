using MediatR;

namespace Zs.Home.ClientApp.Pages.Dashboard.Ping;

public sealed record PingResultQuery : IRequest<PingResult>;
