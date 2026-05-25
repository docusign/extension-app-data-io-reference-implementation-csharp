using System.Text.Json;
using System.Text.Json.Nodes;

namespace ExtensionAppDataIO.Services
{
    public class DataIOFileStore
    {
        private static readonly JsonSerializerOptions WriteOptions = new()
        {
            WriteIndented = true,
        };

        private readonly string _dataRootPath;

        public DataIOFileStore(string dataRootPath)
        {
            _dataRootPath = dataRootPath;
        }

        public List<JsonObject> ReadRecords(string typeName)
        {
            var filePath = GetFilePath(typeName);
            EnsureFileExists(filePath);

            var json = File.ReadAllText(filePath);
            var array = JsonNode.Parse(string.IsNullOrWhiteSpace(json) ? "[]" : json) as JsonArray ?? new JsonArray();

            return array
                .Select(node => node as JsonObject)
                .Where(node => node is not null)
                .Select(node => (JsonObject)node!.DeepClone())
                .ToList();
        }

        public void AppendRecord(string typeName, JsonObject record)
        {
            var records = ReadRecords(typeName);
            records.Add((JsonObject)record.DeepClone());
            WriteRecords(typeName, records);
        }

        public bool PatchRecord(string typeName, string recordId, JsonObject updatedRecord)
        {
            var records = ReadRecords(typeName);
            var existingRecord = records.FirstOrDefault(record => GetStringValue(record["Id"]) == recordId);

            if (existingRecord is null)
            {
                return false;
            }

            foreach (var property in updatedRecord)
            {
                existingRecord[property.Key] = property.Value?.DeepClone();
            }

            WriteRecords(typeName, records);
            return true;
        }

        public JsonObject? FindRelatedRecord(string typeName, string recordId)
        {
            return ReadRecords(typeName).FirstOrDefault(record => GetStringValue(record["Id"]) == recordId);
        }

        private void WriteRecords(string typeName, IEnumerable<JsonObject> records)
        {
            var array = new JsonArray();

            foreach (var record in records)
            {
                array.Add(record.DeepClone());
            }

            File.WriteAllText(GetFilePath(typeName), array.ToJsonString(WriteOptions));
        }

        private string GetFilePath(string typeName)
        {
            return Path.Combine(_dataRootPath, $"{typeName}.json");
        }

        private void EnsureFileExists(string filePath)
        {
            var directoryPath = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "[]");
            }
        }

        public static string? GetStringValue(JsonNode? node)
        {
            if (node is null)
            {
                return null;
            }

            return node switch
            {
                JsonValue value when value.TryGetValue<string>(out var stringValue) => stringValue,
                _ => node.ToJsonString().Trim('"'),
            };
        }
    }
}