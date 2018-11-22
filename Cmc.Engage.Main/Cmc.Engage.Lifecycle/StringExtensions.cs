namespace Cmc.Engage.Lifecycle
{
    public static class StringExtensions
    {
        public static string ExtractActualDomAttribute(this string fullAttribute)
        {
            if (string.IsNullOrWhiteSpace(fullAttribute))
            {
                return null;
            }

            string[] split = fullAttribute.Split('.');
            return split.Length > 1 ? split[split.Length - 1] : null;
        }
    }
}
