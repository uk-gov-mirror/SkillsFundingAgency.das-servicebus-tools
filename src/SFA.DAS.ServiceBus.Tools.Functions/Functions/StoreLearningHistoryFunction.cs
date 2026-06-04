using SFA.DAS.ServiceBus.Tools.Functions.Services;

namespace SFA.DAS.ServiceBus.Tools.Functions.Functions;

public class StoreLearningHistoryFunction(IMessageProcessor messageProcessor)
{
    [Function("StoreLearningHistory")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", "StoreLearningHistory")] HttpRequestData req, FunctionContext functionContext)
    {
        try
        {
            await messageProcessor.SendCommand<StoreLearningHistoryCommand>(req.Body, functionContext);
        }
        catch
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
