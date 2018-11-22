using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Common.Utilities
{
 public static class EntityExtensions
  {
    public static T GetValueOrFallback<T>(this Entity target, Entity fallback, string attributeName)
    {
        if (target != null && target.Contains(attributeName))
        {
            return target.GetAttributeValue<T>(attributeName);
        }
        else if (fallback != null && fallback.Contains(attributeName))
        {
            return fallback.GetAttributeValue<T>(attributeName);
        }

        return default(T);
    }
  }
}
