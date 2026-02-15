using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Zs.Home.Application.Features.Weather.Data;
using Zs.Home.WebApi;

namespace Zs.Home.ClientApp.Pages.Dashboard.Weather;

public sealed class WeatherDashboardQueryHandler : IRequestHandler<WeatherDashboardQuery, WeatherDashboard>
{
    private readonly IWeatherClient _weatherClient;
    private readonly WeatherRegistratorDbContext _dbContext;

    public WeatherDashboardQueryHandler(IWeatherClient weatherClient, WeatherRegistratorDbContext dbContext)
    {
        _weatherClient = weatherClient;
        _dbContext = dbContext;
    }

    public async Task<WeatherDashboard> Handle(WeatherDashboardQuery request, CancellationToken cancellationToken)
    {
        var settings = await _weatherClient.GetAllSettingsAsync(cancellationToken);

        // TODO: _weatherClient.GetHistory вместо запроса к БД отсюда.

        var weatherData = await _dbContext.WeatherData.AsNoTracking()
            .Where(d => d.CreatedAt > DateTime.UtcNow.AddDays(-1))
            .Include(d => d.Source)
            .ThenInclude(s => s.Place)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        if (!weatherData.Any() || DateTime.UtcNow - weatherData.First().CreatedAt > TimeSpan.FromMinutes(15))
            throw new ApplicationException("No actual weather data found");

        return weatherData.ToWeatherDashboard(settings);
    }
}
