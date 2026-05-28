using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ExtensionAppDataIO.Models
{
    public class CreateRecordRequest
    {
        public string typeName { get; set; } = string.Empty;
        public string? recordId { get; set; }
        public string? idempotencyKey { get; set; }
        public JsonObject? data { get; set; }
    }

    public class CreateRecordResponse
    {
        public string recordId { get; set; } = string.Empty;
    }

    public class PatchRecordRequest
    {
        public string typeName { get; set; } = string.Empty;
        public string recordId { get; set; } = string.Empty;
        public JsonObject? data { get; set; }
    }

    public class PatchRecordResponse
    {
        public bool success { get; set; }
    }

    public class SearchRecordsRequest
    {
        public QueryDefinition? query { get; set; }
        public SearchRecordsPagination? pagination { get; set; }
    }

    public class SearchRecordsPagination
    {
        public int limit { get; set; }
        public int skip { get; set; }
    }

    public class SearchRecordsResponse
    {
        public List<JsonObject> records { get; set; } = new();
    }

    public class QueryDefinition
    {
        public List<string> attributesToSelect { get; set; } = new();
        public string from { get; set; } = string.Empty;
        public QueryFilterDefinition? queryFilter { get; set; }
    }

    public class QueryFilterDefinition
    {
        public QueryOperation? operation { get; set; }
    }

    public class QueryOperation
    {
        public QueryOperand? leftOperand { get; set; }

        [JsonPropertyName("operator")]
        public string @operator { get; set; } = string.Empty;

        public QueryOperand? rightOperand { get; set; }
        public QueryOperation? leftOperation { get; set; }
        public QueryOperation? rightOperation { get; set; }
    }

    public class QueryOperand
    {
        public string name { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public bool isLiteral { get; set; }
    }
}