using SFA.DAS.ServiceBus.Tools.Functions.Services;

namespace SFA.DAS.ServiceBus.Tools.Functions.Functions;

public class RefreshEmployerLevyDataCompletedFunction(IMessageProcessor messageProcessor)
{
    [Function("RefreshEmployerLevyDataCompleted")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", "RefreshEmployerLevyDataCompleted")] HttpRequestData req, FunctionContext functionContext)
    {
        try
        {
            await messageProcessor.PublishEvent<RefreshEmployerLevyDataCompletedEvent>(req.Body, functionContext);
        }
        catch
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
