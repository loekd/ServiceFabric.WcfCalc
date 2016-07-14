# ServiceFabric.WcfCalc
Service Fabric Stateless Service hosting a publicly accessible (Web HTTP) WCF service.

## What 
This code sample shows how you can run WCF services in Stateless Services on Azure Service Fabric.

## How

The ICommunicationListener is created using the public name of the cluster on every instance.
The public name needs to be configured as an application parameter.

**Before publishing this application to your cluster, add a cloud configuration file**

Name and path:

*.\ServiceFabric.WcfCalc\ApplicationParameters\Cloud.xml*

The contents need to look like the xml below:
``` xml
<?xml version="1.0" encoding="utf-8"?>
<Application xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Name="fabric:/ServiceFabric.WcfCalc" xmlns="http://schemas.microsoft.com/2011/01/fabric">
  <Parameters>
    <Parameter Name="CalculatorService_InstanceCount" Value="-1" />
    <Parameter Name="CalculatorService_EndpointUrl" Value="mycluster.region.cloudapp.azure.com" />
  </Parameters>
</Application>
```

Replace 'mycluster.region' with the name and region of your own cluster.


### Code

Essential code:
``` javascript
  // first build uri from configuration
  var listener = new WcfCommunicationListener<ICalculatorService>(
    serviceContext: context,
    wcfServiceObject: new WcfCalculatorService(),
    listenerBinding: new WebHttpBinding(WebHttpSecurityMode.None),
    address : new EndpointAddress(uri)
    );
    
    // add WebHttpBehavior
    var ep = listener.ServiceHost.Description.Endpoints.First();
    ep.Behaviors.Add(new WebHttpBehavior());
