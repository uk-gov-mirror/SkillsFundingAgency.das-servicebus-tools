using SFA.DAS.ServiceBus.Tools.Functions.Services;
using SFA.DAS.CommitmentsV2.ExternalHandlers.LearningEvents;

namespace SFA.DAS.ServiceBus.Tools.Functions.Functions;

public class LearningResumedFunction(IMessageProcessor messageProcessor)
{
    [Function("LearningResumed")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", "LearningResumed")] HttpRequestData req, FunctionContext functionContext)
    {
        try
        {
            await messageProcessor.PublishEvent<LearningResumedEvent>(req.Body, functionContext);
        }
        catch
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
