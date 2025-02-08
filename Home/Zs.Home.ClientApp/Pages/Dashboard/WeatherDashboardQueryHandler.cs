using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Zs.Home.Application.Features.Weather.Data;

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
            .Where(d => d.CreatedAt > DateTime.UtcNow.AddDays(-1))
            .Include(d => d.Source)
            .ThenInclude(s => s.Place)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        if (!weatherData.Any() || DateTime.UtcNow - weatherData.First().CreatedAt > TimeSpan.FromMinutes(15))
            throw new ApplicationException("No actual weather data found");

        return weatherData.ToWeatherDashboard(_settings);
    }
}
