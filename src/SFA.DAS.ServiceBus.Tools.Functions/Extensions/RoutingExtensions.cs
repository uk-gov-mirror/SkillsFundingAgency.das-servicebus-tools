using System.Collections.Generic;
using NServiceBus;

namespace SFA.DAS.ServiceBus.Tools.Functions.Extensions;

public static class RoutingExtensions
{
    private const string FinanceMessageHandlersEndpoint = "SFA.DAS.EmployerFinance.MessageHandlers";
    private const string CommitmentsV2MessageHandlersEndpoint = "SFA.DAS.CommitmentsV2.MessageHandlers";

    public static void AddRouting(this RoutingSettings routing)
    {
        RouteToEndpoint([
            typeof(DraftExpireAccountFundsCommand),
            typeof(DraftExpireFundsCommand),
            typeof(ExpireAccountFundsCommand),
            typeof(ExpireFundsCommand),
            typeof(ImportAccountLevyDeclarationsCommand),
            typeof(ImportPaymentsCommand),
            typeof(ImportAccountPaymentsCommand),
            typeof(ProcessPeriodEndPaymentsCommand)
        ], routing, FinanceMessageHandlersEndpoint);

        RouteToEndpoint([
            typeof(StoreLearningHistoryCommand)
        ], routing, CommitmentsV2MessageHandlersEndpoint);
    }

    private static void RouteToEndpoint(List<Type> types, RoutingSettings routing, string endpointName)
    {
        foreach (var type in types)
        {
            routing.RouteToEndpoint(type, endpointName);
        }
    }
}
