using Newtonsoft.Json;

namespace AuditLogMigration.DataModel
{
    public class QueryItem
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("sqlstatement")]
        public string SqlStatement { get; set; }
    }
}
