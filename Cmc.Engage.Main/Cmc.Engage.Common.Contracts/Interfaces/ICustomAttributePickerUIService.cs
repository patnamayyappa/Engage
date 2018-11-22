
using Cmc.Core.Xrm.ServerExtension.Core;

namespace Cmc.Engage.Common
{
    public interface ICustomAttributePickerUIService
    {
        void RetrieveAttributesRelationshipsForEntities(IExecutionContext context);
        void RetrieveLocalizedAttributeNamesToDisplay(IExecutionContext context);
        void RetrieveUserLookupsForEntity(IExecutionContext context);        
    }
}