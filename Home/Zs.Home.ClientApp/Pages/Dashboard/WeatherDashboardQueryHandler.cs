using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Zs.Home.Application.Features.Weather;
using Zs.Home.Application.Features.Weather.Data;
using static Zs.Home.ClientApp.Pages.Dashboard.Constants;

namespace Zs.Home.ClientApp.Pages.Dashboard;

public sealed class WeatherDashboardQueryHandler : IRequestHandler<WeatherDashboardQuery, WeatherDashboard>
{
    private readonly WeatherRegistratorDbContext _dbContext;
    private readonly WeatherDashboardSettings _settings;

    public WeatherDashboardQueryHandler(WeatherRegistratorDbContext dbContext, IOptions<WeatherDashboardSettings> options)
    {
        _dbContext = dbContext;
        _settings = options.Value;
    }

    public async Task<WeatherDashboard> Handle(WeatherDashboardQuery request, CancellationToken cancellationToken)
    {
        var weatherData = await _dbContext.WeatherData.AsNoTracking()
            .Include(d => d.Source)
            .ThenInclude(s => s.Place)
            .OrderByDescending(d => d.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        if (!weatherData.Any() || DateTime.UtcNow - weatherData.First().CreatedAt > TimeSpan.FromMinutes(15))
            throw new ApplicationException("No actual weather data found");

        // На данный момент график не строится, поэтому берём только последние значения
        var sourceCount = weatherData.Select(d => d.SourceId).Distinct().Count();
        weatherData = weatherData
            .OrderByDescending(d => d.CreatedAt)
            .ThenBy(d => d.SourceId)
            .Take(sourceCount)
            .ToList();


        return weatherData.ToWeatherDashboard(_settings);
    }
}
