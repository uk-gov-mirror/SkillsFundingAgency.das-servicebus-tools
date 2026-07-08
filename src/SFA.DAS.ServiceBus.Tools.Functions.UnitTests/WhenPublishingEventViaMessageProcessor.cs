using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.ServiceBus.Tools.Functions.Services;

namespace SFA.DAS.ServiceBus.Tools.Functions.UnitTests;

public class WhenPublishingEventViaMessageProcessor
{
    [Test]
    public async Task Then_Endpoint_Publish_Is_Called()
    {
        var endpoint = new Mock<IFunctionEndpoint>();
        var logger = new Mock<ILogger<MessageProcessor>>();
        var sut = new MessageProcessor(endpoint.Object, logger.Object);
        var context = Mock.Of<FunctionContext>();
        var payload = "{\"AccountId\":123,\"ApprenticeshipEmployerType\":1}";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));

        await sut.PublishEvent<ApprenticeshipEmployerTypeChangeEvent>(stream, context);

        endpoint.Verify(
            x => x.Publish(
                It.Is<ApprenticeshipEmployerTypeChangeEvent>(e => e.AccountId == 123),
                context),
            Times.Once);
    }

    [Test]
    public void Then_Exception_Is_Thrown_When_Publish_Fails()
    {
        var endpoint = new Mock<IFunctionEndpoint>();
        endpoint
            .Setup(x => x.Publish(It.IsAny<ApprenticeshipEmployerTypeChangeEvent>(), It.IsAny<FunctionContext>()))
            .ThrowsAsync(new Exception("publish failed"));
        var logger = new Mock<ILogger<MessageProcessor>>();
        var sut = new MessageProcessor(endpoint.Object, logger.Object);
        var context = Mock.Of<FunctionContext>();
        var payload = "{\"AccountId\":123,\"ApprenticeshipEmployerType\":1}";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));

        Assert.ThrowsAsync<Exception>(() => sut.PublishEvent<ApprenticeshipEmployerTypeChangeEvent>(stream, context));
    }
}
