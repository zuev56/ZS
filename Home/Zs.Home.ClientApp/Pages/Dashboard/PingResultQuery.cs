using MediatR;

namespace Zs.Home.ClientApp.Pages.Dashboard;

public sealed record PingResultQuery : IRequest<PingResult>;
