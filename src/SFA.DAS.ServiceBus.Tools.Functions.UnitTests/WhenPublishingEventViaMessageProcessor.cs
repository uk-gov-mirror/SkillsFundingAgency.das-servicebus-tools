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
using SFA.DAS.EmployerFinance.Messages.Events;
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

    [Test]
    public async Task WhenPublishingRefreshEmployerLevyDataCompletedEvent_Then_Endpoint_Publish_Is_Called()
    {
        // Arrange
        var endpoint = new Mock<IFunctionEndpoint>();
        var logger = new Mock<ILogger<MessageProcessor>>();
        var sut = new MessageProcessor(endpoint.Object, logger.Object);
        var context = Mock.Of<FunctionContext>();
        var payload = "{\"AccountId\":1001,\"PayeRef\":\"123/AB45678\",\"LastLevyDeclarationDate\":\"2024-03-15T00:00:00\",\"PeriodMonth\":6,\"PeriodYear\":\"2526\",\"LevyImported\":false,\"LevyTransactionValue\":0,\"Created\":\"2026-06-20T10:00:00Z\"}";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));

        // Act
        await sut.PublishEvent<RefreshEmployerLevyDataCompletedEvent>(stream, context);

        // Assert
        endpoint.Verify(
            x => x.Publish(
                It.Is<RefreshEmployerLevyDataCompletedEvent>(e =>
                    e.AccountId == 1001 &&
                    e.PayeRef == "123/AB45678" &&
                    e.LastLevyDeclarationDate == new DateTime(2024, 3, 15) &&
                    e.PeriodMonth == 6 &&
                    e.PeriodYear == "2526" &&
                    !e.LevyImported &&
                    e.LevyTransactionValue == 0 &&
                    e.Created == new DateTime(2026, 6, 20, 10, 0, 0, DateTimeKind.Utc)),
                context),
            Times.Once);
    }

    [Test]
    public void WhenPublishingRefreshEmployerLevyDataCompletedEvent_And_Publish_Fails_Then_Exception_Is_Thrown()
    {
        // Arrange
        var endpoint = new Mock<IFunctionEndpoint>();
        endpoint
            .Setup(x => x.Publish(It.IsAny<RefreshEmployerLevyDataCompletedEvent>(), It.IsAny<FunctionContext>()))
            .ThrowsAsync(new Exception("publish failed"));
        var logger = new Mock<ILogger<MessageProcessor>>();
        var sut = new MessageProcessor(endpoint.Object, logger.Object);
        var context = Mock.Of<FunctionContext>();
        var payload = "{\"AccountId\":1001,\"Created\":\"2026-06-20T10:00:00Z\"}";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));

        // Act // Assert
        Assert.ThrowsAsync<Exception>(() => sut.PublishEvent<RefreshEmployerLevyDataCompletedEvent>(stream, context));
    }
}
