using System;

namespace ChatAdmin.Bot.Models;

internal sealed class SfProcessGroupMessageResult
{
    public SfResultAction? Action { get; set; }
    public string MessageText { get; set; }
    public DateTime? AccountingStartDate { get; set; }
    public int? BanId { get; set; }
}
