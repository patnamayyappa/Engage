<Query Kind="Program">
  <NuGetReference>Microsoft.CrmSdk.CoreAssemblies</NuGetReference>
  <NuGetReference>Microsoft.CrmSdk.XrmTooling.CoreAssembly</NuGetReference>
  <NuGetReference>SonomaPartners.Crm.Toolkit</NuGetReference>
  <Namespace>Microsoft.Xrm.Sdk.Query</Namespace>
  <Namespace>Microsoft.Xrm.Tooling.Connector</Namespace>
  <Namespace>SonomaPartners.Crm.Toolkit</Namespace>
  <Namespace>Microsoft.Xrm.Sdk</Namespace>
  <Namespace>Microsoft.Xrm.Sdk.Messages</Namespace>
</Query>

void Main()
{
	var serviceClient = new CrmServiceClient("AuthType=Office365;RequiresNewInstance=true;Url=https://auroracrm9dev.crm.dynamics.com;Username=ehirsch@auroracrm9dev.onmicrosoft.com;Password=PASSWORD");
	var records = serviceClient.RetrieveMultipleAll(
	@"<fetch>
	    <entity name='cmc_todo'>
		  <attribute name='cmc_internalexternal' />
		  <attribute name='cmc_assignedtoid' />
		  <filter type='or'>
		    <condition attribute='cmc_internalexternal' operator='not-null' />
			<condition attribute='cmc_assignedtoid' operator='not-null' />
		  </filter>
		</entity>
	  </fetch>").Entities;
	
	var requests = new List<OrganizationRequest>();
	foreach (var record in records)
	{
		requests.Add(new UpdateRequest()
		{
			Target = new Entity("cmc_todo")
			{
				Attributes =
				{
					{"cmc_ownershiptype", record.GetAttributeValue<OptionSetValue>("cmc_internalexternal")},
					{"cmc_assignedtostudentid", record.GetAttributeValue<EntityReference>("cmc_assignedtoid")}
				},
				Id = record.Id
			}
		});
	}
	
	serviceClient.ExecuteMultipleAll(requests);
}

// Define other methods and classes here