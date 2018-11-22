using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Common.Activities
{
    /// <summary>
    /// This step retrieves the default Configuration entity.
    /// </summary>
    public class RetrieveConfigurationValueActivity : ActivityBase
    {
        protected override void Execute(IActivityExecutionContext context)
        {
            var logic = context.IocScope.Resolve<IConfigurationService>();
            var value = logic.GetActiveConfiguration();
            ConfigurationValue.Set(context.ActivityContext, value.ToEntityReference());
        }

        /// <summary>
        /// Returns an active configuration.
        /// </summary>
        [ReferenceTarget(cmc_configuration.EntityLogicalName)]
        [Output("ConfigurationValue")]
        public OutArgument<EntityReference> ConfigurationValue { get; set; }
    }
}
