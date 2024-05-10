namespace Zs.VkActivity.WebApi.Models;

public sealed class ListUserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsOnline { get; set; }
    public int ActivitySec { get; internal set; }
}