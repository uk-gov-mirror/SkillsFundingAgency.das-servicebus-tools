using SFA.DAS.ServiceBus.Tools.Functions.Services;

namespace SFA.DAS.ServiceBus.Tools.Functions.Functions;

public class ApprovedLearningUpdatedFunction(IMessageProcessor messageProcessor)
{
    [Function("ApprovedLearningUpdated")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", "ApprovedLearningUpdated")] HttpRequestData req, FunctionContext functionContext)
    {
        try
        {
            await messageProcessor.PublishEvent<ApprovedLearningUpdatedEvent>(req.Body, functionContext);
        }
        catch
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
