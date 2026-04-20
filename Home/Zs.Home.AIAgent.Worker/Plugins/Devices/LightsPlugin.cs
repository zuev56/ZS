using System.ComponentModel;
using Microsoft.SemanticKernel;
using Zs.Home.AIAgent.Worker.Plugins.Devices.Models;

namespace Zs.Home.AIAgent.Worker.Plugins.Devices;

/// <summary>
/// Тестовый плагин, будет удалён.
/// </summary>
public sealed class LightsPlugin
{
    public LightsPlugin()
    {
        Console.WriteLine("Lights plugin loaded");
    }

    private readonly List<DiscreteControl> _lights =
    [
        new() {Id = 1, Name = "Настольная лампа", IsOn = false},
        new() {Id = 2, Name = "Спальня", IsOn = true},
        new() {Id = 3, Name = "Гостинная", IsOn = false},
        new() {Id = 4, Name = "Кухня", IsOn = true},
        new() {Id = 5, Name = "Туалет", IsOn = false},
        new() {Id = 6, Name = "Детская", IsOn = false}
    ];

    [KernelFunction("get_lights")]
    [Description("Получить список светильников и их текущее состояние")]
    [return: Description("Список светильников")]
    public Task<List<DiscreteControl>> GetLightsAsync() => Task.FromResult(_lights);

    [KernelFunction("change_state")]
    [Description("Изменить состояние светильника")]
    [return: Description("Обновлённое состояние светильника; Вернёт null если светильник не существует")]
    public Task<DiscreteControl?> ChangeStateAsync(int id, bool isOn)
    {
        var light = _lights.FirstOrDefault(light => light.Id == id);

        if (light == null)
            return Task.FromResult<DiscreteControl?>(null);

        light.IsOn = isOn;

        return Task.FromResult(light);
    }
}
