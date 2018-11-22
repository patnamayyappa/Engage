namespace Cmc.Engage.Retention
{
    public class RelatedEntity
    {
        public string EntityName { get; set; }
        public string FromAttribute { get; set; }
        public string ToAttribute { get; set; }

        public override string ToString()
        {
            return $"{EntityName}({FromAttribute}={ToAttribute})";
        }
    }
}
