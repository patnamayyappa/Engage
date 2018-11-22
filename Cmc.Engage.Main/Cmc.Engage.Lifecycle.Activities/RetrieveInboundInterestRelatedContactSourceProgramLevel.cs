using System.Activities;
using System.ComponentModel;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// This step retrieves the source Program Level of the Inbound Interest's contact.
    /// </summary>

    [DisplayName("Get source Program Level of Inbound Interest's Contact")]
    public class RetrieveInboundInterestRelatedContactSourceProgramLevel : RetrieveInboundInterestRelatedContactField
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            Execute(executionContext, "cmc_srcprgmlevelid", SourceProgramLevelId);
        }

        /// <summary>
        /// The source Program Level of the Inbound Interest's contact.
        /// </summary>
        [ReferenceTarget("mshied_programlevel")]
        [RequiredArgument]
        [Output("Source Program Level")]
        public OutArgument<EntityReference> SourceProgramLevelId { get; set; }
    }
}
