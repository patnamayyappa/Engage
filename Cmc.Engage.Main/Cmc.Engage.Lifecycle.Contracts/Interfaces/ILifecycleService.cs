using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Lifecycle
{
    public interface ILifecycleService
    {
        EntityReference RetrieveContactOpenLifecycle(RetrieveContactOpenLifecycleFilters filters);
    }
}
