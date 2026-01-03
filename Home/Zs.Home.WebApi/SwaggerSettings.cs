namespace Zs.Home.WebApi;

// TODO: Перенести в Common.WebApi
public class SwaggerSettings
{
    private const string SectionName = "Swagger";

    public const string ApiTitle = $"{SectionName}:{nameof(ApiTitle)}";
    public const string ApiVersion = $"{SectionName}:{nameof(ApiVersion)}";
    public const string EndpointUrl = $"{SectionName}:{nameof(EndpointUrl)}";
}
