using Cmc.Core.Xrm.ServerExtension.Core;
using System.Collections.Generic;

namespace Cmc.Engage.Common
{
    public interface IRetrieveConfigurationValueService
    {
        string RunActivity(IActivityExecutionContext executionContext, List<object> configurationName);
    }
}
