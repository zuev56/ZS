using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Zs.Bot.Data.Models;
using Zs.Bot.Telegram.Extensions;

namespace Zs.Home.Bot;

internal sealed class BotSettings
{
    public const string SectionName = "Bot";

    [Required]
    public string Token { get; init; } = null!;

    public string? Name { get; init; }

    [Required]
    public long OwnerChatRawId { get; init; }

    [Required]
    public string CliPath { get; init; } = null!;

    [Required]
    public IReadOnlyList<long> PrivilegedUserRawIds { get; init; } = Array.Empty<long>();

    public static readonly Func<Message, string?> GetMessageText = static message => message.GetText();
}