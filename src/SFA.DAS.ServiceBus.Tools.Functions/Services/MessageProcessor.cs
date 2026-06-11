using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NServiceBus;

namespace SFA.DAS.ServiceBus.Tools.Functions.Services;

public interface IMessageProcessor
{
    Task SendCommand<T>(FunctionContext context);
    Task SendCommand<T>(Stream requestContent, FunctionContext context);
    Task PublishEvent<T>(Stream requestContent, FunctionContext context);
}

public class MessageProcessor(IFunctionEndpoint endpoint, ILogger<MessageProcessor> logger) : IMessageProcessor
{
    public async Task SendCommand<T>(Stream requestContent, FunctionContext context) 
    {
        using var reader = new StreamReader(requestContent);
        var content = await reader.ReadToEndAsync();
        var command = JsonConvert.DeserializeObject<T>(content);
        var typeName = typeof(T).ToString().Split('.').Last();

        logger.LogInformation("Sending Message '{TypeName)}' with payload: {Payload}", typeName, content);

        await SendCommand(command, typeName, context);
    }

    public async Task SendCommand<T>(FunctionContext context)
    {
        var typeName = typeof(T).ToString().Split('.').Last();

        logger.LogInformation("Sending Message '{TypeName)}'.", typeName);

        var command = Activator.CreateInstance<T>();

        await SendCommand(command, typeName, context);
    }

    public async Task PublishEvent<T>(Stream requestContent, FunctionContext context)
    {
        using var reader = new StreamReader(requestContent);
        var content = await reader.ReadToEndAsync();
        var @event = JsonConvert.DeserializeObject<T>(content);
        var typeName = typeof(T).ToString().Split('.').Last();

        logger.LogInformation("Publishing event '{TypeName)}' with payload: {Payload}", typeName, content);

        await PublishEvent(@event, typeName, context);
    }

    private async Task SendCommand<T>(T command, string typeName, FunctionContext context)
    {
        try
        {
            await endpoint.Send(command, context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to send command {TypeName}.", typeName);
            throw;
        }

        logger.LogInformation("Message '{TypeName}' sent successfully", typeName);
    }

    private async Task PublishEvent<T>(T @event, string typeName, FunctionContext context)
    {
        try
        {
            await endpoint.Publish(@event, context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to publish event {TypeName}.", typeName);
            throw;
        }

        logger.LogInformation("Event '{TypeName}' published successfully", typeName);
    }
}