namespace Zs.Parser.EspMeteo;

internal static class StringExtensions
{
    internal static string RepairXml(this string brokenHtml)
    {
        return brokenHtml
            .Replace("<br>", "<br/>")
            .Replace("<hr>", "<hr/>")
            .Replace("<link rel=\"stylesheet\" href=\"main.css\">", "<link rel=\"stylesheet\" href=\"main.css\"/>")
            .Replace("<meta http-equiv=\"REFRESH\" content=\"60\">", "meta http-equiv=\"REFRESH\" content=\"60\"/>")
            .Replace("&deg;", "°")
            .Replace("</body>", "</div></body>");
    }
}