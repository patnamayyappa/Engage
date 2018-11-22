using System;
using System.Collections.Generic;
using Autofac;
using Cmc.Core.Xrm.ServerExtension.Functions;
using Cmc.Engage.FunctionExtensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Cmc.Engage.Common.Functions
{
    public static class AddressGeoCoder
    {
        [FunctionName("AddressGeoCoder")]
        public static void Run([TimerTrigger("%AddressGeoCoderSchedule%")]TimerInfo myTimer, TraceWriter log, ExecutionContext context)
        {          
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            var registrationModulesList = new List<Type>
            {
                typeof(CommonRegistrationModule)               
            };
            var container = FunctionExtensions.FunctionExtensions.GetServiceLocator(registrationModulesList);
            var addressService = container.Resolve<IAddressService>();
            addressService.AddressGeoCoderLogic();

        }

    }
}
