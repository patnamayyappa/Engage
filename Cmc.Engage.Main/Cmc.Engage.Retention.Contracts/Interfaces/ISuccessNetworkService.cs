using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Retention
{
    public interface ISuccessNetworkService
    {
        void SuccessNetworkAssignment(IOrganizationService organizationService);
    }
}
