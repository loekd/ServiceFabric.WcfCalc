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

            yield return new ServiceInstanceListener(context =>
            {
                return CreateRestListener(context);
                //return CreateSoapListener(context);
            });          
        }

        private static ICommunicationListener CreateRestListener(StatelessServiceContext context)
        {
            string host = context.NodeContext.IPAddressOrFQDN;
            var endpointConfig = context.CodePackageActivationContext.GetEndpoint("CalculatorEndpoint");
            int port = endpointConfig.Port;
            string scheme = endpointConfig.Protocol.ToString();
            string uri = string.Format(CultureInfo.InvariantCulture, "{0}://{1}:{2}/", scheme, host, port);
            var listener = new WcfCommunicationListener<ICalculatorService>(
                serviceContext: context,
                wcfServiceObject: new WcfCalculatorService(),
                listenerBinding: new WebHttpBinding(WebHttpSecurityMode.None),
                address: new EndpointAddress(uri)
            );
            var ep = listener.ServiceHost.Description.Endpoints.Last();
            ep.Behaviors.Add(new WebHttpBehavior());
            return listener;
        }

        private static ICommunicationListener CreateSoapListener(StatelessServiceContext context)
        {
            string host = context.NodeContext.IPAddressOrFQDN;
            var endpointConfig = context.CodePackageActivationContext.GetEndpoint("CalculatorEndpoint");
            int port = endpointConfig.Port;
            string scheme = endpointConfig.Protocol.ToString();

            string uri = string.Format(CultureInfo.InvariantCulture, "{0}://{1}:{2}/", scheme, host, port);
            var listener = new WcfCommunicationListener<ICalculatorService>(
                serviceContext: context,
                wcfServiceObject: new WcfCalculatorService(),
                listenerBinding: new BasicHttpBinding(BasicHttpSecurityMode.None),
                address: new EndpointAddress(uri)
            );

            // Check to see if the service host already has a ServiceMetadataBehavior
            ServiceMetadataBehavior smb = listener.ServiceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
            // If not, add one
            if (smb == null)
            {
                smb = new ServiceMetadataBehavior();
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                smb.HttpGetEnabled = true;
                smb.HttpGetUrl = new Uri(uri);

                listener.ServiceHost.Description.Behaviors.Add(smb);
            }
            return listener;
        }

        private static string EndpointTypeFromConfig(StatelessServiceContext context)
        {
            var configSection = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            var section = (configSection?.Settings.Sections.Contains("Endpoint") ?? false) ? configSection?.Settings.Sections["Endpoint"] : null;
            string endPointType = (section?.Parameters.Contains("EndpointType") ?? false) ? section.Parameters["EndpointType"].Value : null;
            return endPointType;
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
