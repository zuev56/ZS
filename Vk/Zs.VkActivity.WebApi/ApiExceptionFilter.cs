using System;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Zs.Common.Exceptions;
using Zs.Common.Extensions;

namespace Zs.VkActivity.WebApi;

// TODO: Move to Zs.Common.Web
public sealed class ApiExceptionFilter : Attribute, IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogErrorIfNeed(context.Exception, $"Action {context.ActionDescriptor.DisplayName} error");

        context.Result = context.Exception is FaultException faultException
            // TODO: разобраться с кодами ошибок
            ? new ContentResult {StatusCode = 500, Content = JsonSerializer.Serialize(faultException.Fault)}
            : new ContentResult {StatusCode = 500, Content = "Internal Server Error"};

        context.ExceptionHandled = true;
    }
}
