using System;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Common.Utilities
{
    public static class PluginExecutionContextExtensions
    {
        public static bool IsValidCall(this IPluginExecutionContext context, string entityName)
        {
            if (!context.InputParameters.Contains("Target")) return false;
            if (!(context.InputParameters["Target"] is Entity)) return false;
            var entity = (Entity)context.InputParameters["Target"];
            return entity.LogicalName == entityName;
        }
        public static Guid? ParseGuidInput(this IPluginExecutionContext context, string inputName)
        {
            Guid inputGuid = Guid.Empty;
            string inputString = context.InputParameters.Contains(inputName) ? (string)context.InputParameters[inputName] : null;

            if (string.IsNullOrWhiteSpace(inputString))
            {
                return null;
            }

            if (!Guid.TryParse(inputString, out inputGuid))
            {
                throw new InvalidPluginExecutionException($"Invalid input, unable to cast {nameof(inputName)} to Guid");
            }

            return inputGuid;
        }

        public static DateTime ParseIso8601DateInput(this IPluginExecutionContext context, string inputName)
        {
            string inputDate = (string)context.InputParameters[inputName];
            return inputDate.FromIso8601Date();
        }

    }
}
