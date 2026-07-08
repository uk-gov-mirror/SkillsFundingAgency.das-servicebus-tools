using Microsoft.Extensions.Hosting;
using NServiceBus;

namespace SFA.DAS.ServiceBus.Tools.Functions.Extensions;

public static class NServiceBusExtensions
{
    private const string EndpointName = "SFA.DAS.ServiceBus.Tools.Functions";
    private const string ErrorEndpointName = $"{EndpointName}-error";

    public static IHostBuilder ConfigureNServiceBus(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseNServiceBus(EndpointName, (config, endpointConfiguration) =>
        {
            endpointConfiguration.Routing.AddRouting();
            endpointConfiguration.AdvancedConfiguration.AssemblyScanner().ScanFileSystemAssemblies = false;
            endpointConfiguration.AdvancedConfiguration.SendFailedMessagesTo(ErrorEndpointName);
            endpointConfiguration.AdvancedConfiguration.Conventions()
                .DefiningCommandsAs(IsCommand)
                .DefiningEventsAs(t => t.Name.EndsWith("Event"));

            if (!string.IsNullOrEmpty(config["NServiceBusLicense"]))
            {
                var decodedLicence = WebUtility.HtmlDecode(config["NServiceBusLicense"]);
                endpointConfiguration.AdvancedConfiguration.License(decodedLicence);
            }

#if DEBUG
            var transport = endpointConfiguration.AdvancedConfiguration.UseTransport<LearningTransport>();
            transport.StorageDirectory(Path.Combine(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src")), @"src\.learningtransport"));
#endif
        });

        return hostBuilder;
    }
    
    private static bool IsCommand(Type t) => t is ICommand || IsDasMessage(t, "Messages.Commands");

    private static bool IsDasMessage(Type t, string namespaceSuffix)
        => t.Namespace != null &&
           t.Namespace.StartsWith("SFA.DAS") &&
           t.Namespace.EndsWith(namespaceSuffix);
}
