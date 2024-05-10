namespace Zs.VkActivity.WebApi.Models;

public sealed class VisitInfoDto
{
    public string Platform { get; set; } = null!;
    public int Count { get; set; }
    public string Time { get; set; } = null!;
}
