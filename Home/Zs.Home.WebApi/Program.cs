var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


/*
 * Weather // Для вывода информации с устройств из разных сфер выгоднее иметь обобщённый DeviceController
 *      EspMeteoStatus(IP)
 *        -Temperature
 *        -Humidity
 *        -Pressure
 *      Forecast|CurrentOutside

 * Seq
 *      Week
 *      24 hours
 *      12 hours
 *      6 hours
 *      Last hour
 *
 * OS
 *		Journal analyzis
 *
 * Hardware // DeviceController
 *      CPU temperature
 *      CPU usage
 *      Memory usage
 *
 * Services
 *      HealthCheck
 *      Stop
 *      Start
 *      Restart
 *
 * Network
 *      Device list
 *      Unknown devices
 *
 * Ping
 *      IP
 *      IP:Port
 *
 */
