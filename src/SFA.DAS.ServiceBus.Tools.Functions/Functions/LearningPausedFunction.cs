using SFA.DAS.ServiceBus.Tools.Functions.Services;

namespace SFA.DAS.ServiceBus.Tools.Functions.Functions;

public class LearningPausedFunction(IMessageProcessor messageProcessor)
{
    [Function("LearningPaused")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", "LearningPaused")] HttpRequestData req, FunctionContext functionContext)
    {
        try
        {
            await messageProcessor.PublishEvent<LearningPausedEvent>(req.Body, functionContext);
        }
        catch
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
