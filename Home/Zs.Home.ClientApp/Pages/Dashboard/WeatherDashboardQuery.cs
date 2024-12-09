using MediatR;

namespace Zs.Home.ClientApp.Pages.Dashboard;

public sealed record WeatherDashboardQuery : IRequest<WeatherDashboard>;
