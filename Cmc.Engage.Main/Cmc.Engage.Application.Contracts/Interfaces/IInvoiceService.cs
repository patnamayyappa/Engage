using Cmc.Core.Xrm.ServerExtension.Core;

namespace Cmc.Engage.Application
{
    public interface IInvoiceService
    {
        void AddApplicationFeesToInvoice(IExecutionContext executionContext);
    }
}
