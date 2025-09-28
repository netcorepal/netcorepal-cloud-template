using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using NetCorePal.Extensions.DistributedLocks;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Endpoints.DemoEndpoints;

[Tags("Demo")]
[HttpGet("/demo/lock")]
[AllowAnonymous]
public class LockEndpoint(IDistributedLock distributedLock) : EndpointWithoutRequest<ResponseData<bool>>
{
    private static bool _isRunning = false;

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (_isRunning)
        {
            await Send.OkAsync(true.AsResponseData(), cancellation: ct);
            return;
        }

        await using var handle = await distributedLock.AcquireAsync("lock");
        if (_isRunning)
        {
            await Send.OkAsync(true.AsResponseData(), cancellation: ct);
            return;
        }

#pragma warning disable S2696
        _isRunning = true;
#pragma warning restore S2696
        await Task.Delay(1000, ct);
#pragma warning disable S2696
        _isRunning = false;
#pragma warning restore S2696 
        await Send.OkAsync(false.AsResponseData(), cancellation: ct);
    }
}
