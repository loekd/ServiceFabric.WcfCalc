using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Globalization;

namespace CalculatorService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CalculatorService : StatelessService
    {
        public CalculatorService(StatelessServiceContext context)
            : base(context)
        { }
              
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] { new ServiceInstanceListener((context) =>
            {
                string host = HostFromConfig(context);
                if (string.IsNullOrWhiteSpace(host))
                {
                     host = context.NodeContext.IPAddressOrFQDN;
                }
               
                var endpointConfig = context.CodePackageActivationContext.GetEndpoint("CalculatorEndpoint");
                int port= endpointConfig.Port;
                string scheme = endpointConfig.Protocol.ToString();
                string uri = string.Format(CultureInfo.InvariantCulture, "{0}://{1}:{2}", scheme, host, port);

                var listener = new WcfCommunicationListener<ICalculatorService>(
                    serviceContext: context,
                    wcfServiceObject: new WcfCalculatorService(),
                    listenerBinding: new WebHttpBinding(WebHttpSecurityMode.None),
                    address : new EndpointAddress(uri)
                );

                var ep = listener.ServiceHost.Description.Endpoints.First();
                ep.Behaviors.Add(new WebHttpBehavior());

                return listener;
            }
        )};

        }

        private static string HostFromConfig(StatelessServiceContext context)
        {
            var configSection = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            var section = (configSection?.Settings.Sections.Contains("ClusterLocation") ?? false) ? configSection?.Settings.Sections["ClusterLocation"] : null;
            string host = (section?.Parameters.Contains("EndpointUrl") ?? false) ? section.Parameters["EndpointUrl"].Value : null;
            return host;
        }
    }
        
    internal class WcfCalculatorService : ICalculatorService
    {
        public Task<int> Add(int a, int b)
        {
            return Task.FromResult(a + b);
        }
    }
}
