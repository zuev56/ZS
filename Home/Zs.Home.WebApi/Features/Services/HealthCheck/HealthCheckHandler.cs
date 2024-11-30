using MediatR;

namespace Zs.Home.WebApi.Features.Services.HealthCheck;

public sealed class HealthCheckHandler : IRequestHandler<HealthCheckRequest, HealthCheckResponse>
{
    public async Task<HealthCheckResponse> Handle(HealthCheckRequest request, CancellationToken cancellationToken)
    {
        /* TODO
         - Время последней операции в Боте, Джобах
         - Пинг бота, джобов
         - Проверка работоспособности сервисов/контейнеров
         - Потребляемые ресурсы ботом, джобом, апи, клиентским приложением
         */
        throw new System.NotImplementedException();
    }
}
