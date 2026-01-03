using MediatR;

namespace Zs.Home.ClientApp.Pages.Dashboard.Weather;

public sealed record WeatherDashboardQuery : IRequest<WeatherDashboard>;
