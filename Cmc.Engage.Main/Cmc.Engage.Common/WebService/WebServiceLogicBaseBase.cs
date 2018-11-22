using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Common
{
    public abstract class WebServiceLogicBase
    {
        /// <summary>
        /// Performs web service logic.
        /// </summary>
        /// <param name="serviceProvider">Standard service provider passed natively to all plugins.</param>
        /// <param name="inputData">JSON serialized input</param>
        /// <returns>Result of web service logic.</returns>
        public abstract object DoWork(ExecutionContext context, string inputData);
        /// <summary>
        /// Deserializes a JSON serialized input into the specified type
        /// </summary>
        /// <typeparam name="T">Type to deserialize into.</typeparam>
        /// <param name="inputData">JSON serialized input passed in from the web service entry point plugin</param>
        /// <returns>Deserialized inputData</returns>
        protected static T GetInput<T>(string inputData)
        {
            try
            {
                // DataContractJsonSerializer is used instead of JavaScriptSerializer because it works in sandbox mode as well.
                // If T is a class, it should be marked as [DataContract] with [DataMember] for serializable attributes.
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(inputData)))
                {
                    var ser = new DataContractJsonSerializer(typeof(T));
                    var result = (T)ser.ReadObject(ms);
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(String.Format("Unable to deserialize the value '{0}' to the type '{1}'.",
                    inputData, typeof(T).ToString()), ex);
            }
        }
    }
}
