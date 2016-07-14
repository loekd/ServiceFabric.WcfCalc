# ServiceFabric.WcfCalc
Service Fabric Stateless Service hosting a publicly accessible (Web HTTP) WCF service.

## What 
This code sample shows how you can run WCF services in Stateless Services on Azure Service Fabric.

Use the overload of the constructor of WcfCommunicationListener that takes 'address' instead of 'endpointResourceName', so you can influence on what URL the WCF service will be listening.

## How to create a WCF Service with a WebHttpBinding (REST):

``` javascript
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
```

## How to create a WCF Service with a BasicHttpBinding (SOAP):
``` javascript
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

  // (Optional) Check to see if the service host already has a ServiceMetadataBehavior
  ServiceMetadataBehavior smb = listener.ServiceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
  if (smb == null)
  {
    smb = new ServiceMetadataBehavior();
    smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
    smb.HttpGetEnabled = true;
    smb.HttpGetUrl = new Uri(uri);
    listener.ServiceHost.Description.Behaviors.Add(smb);
}
return listener;

```

## Configuration

The line
``` javascript
context.CodePackageActivationContext.GetEndpoint("CalculatorEndpoint")
```
reads its data from the file 'ServiceManifest.xml'
Make sure the port mentioned here matches a rule in the load balancer.


```xml
<Resources>
    <Endpoints>
      <!-- This endpoint is used by the communication listener to obtain the port on which to 
           listen. Please note that if your service is partitioned, this port is shared with 
           replicas of different partitions that are placed in your code. -->
      <Endpoint Name="CalculatorEndpoint" Protocol="http" Type="Input" Port="80" />
    </Endpoints>
  </Resources>
  ```
