using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Core;

namespace Cmc.Engage.Communication
{
    public interface IRetrieveDepartmentPhoneNumberService
    {
        string RunActivity(IActivityExecutionContext executionContext, List<object> deptId);
    }
}
