﻿using System;

namespace Zs.Home.Application.Features.Seq;

public sealed record LogEntry
{
    public required DateTime Timestamp { get; init; }
    public required string Level { get; init; }
    public required string? ApplicationName { get; init; }
    public required string Message { get; init; }
    /// <summary>
    /// Шаблон сообщения, для выявления одинаковых записей, но с различающимися параметрами
    /// </summary>
    public required string MessagePattern { get; init; }
    public object? AdditionalData { get; init; }

    public override string ToString()
        => $"[{Timestamp}] [{Level}] {Message} {(AdditionalData != null ? AdditionalData.ToString() : "")}";
}
