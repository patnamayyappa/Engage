
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common
{
  public  interface IAddressService
    {
        void GeocodeAddress(IExecutionContext context);     
        void CheckAndValidatePrimaryAddress(IExecutionContext context);

        void AddressGeoCoderLogic();
        void SetAddressStateorCountry(IExecutionContext context);
    }
}
