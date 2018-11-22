using System;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common
{
    public abstract class PortalWebServiceLogicBase
    {
        public abstract object DoWork(IExecutionContext context, string inputData);
        protected static T GetInput<T>(string inputData)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(inputData);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(String.Format("Unable to deserialize the value '{0}' to the type '{1}'.",
                    inputData, typeof(T).ToString()), ex);
            }
        }
    }
}
