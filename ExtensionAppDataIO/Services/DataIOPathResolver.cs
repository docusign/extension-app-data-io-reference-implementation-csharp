using System.Text.Json.Nodes;

namespace ExtensionAppDataIO.Services
{
    public class DataIOPathResolver
    {
        private readonly DataIOFileStore _fileStore;

        public DataIOPathResolver(DataIOFileStore fileStore)
        {
            _fileStore = fileStore;
        }

        public JsonNode? ResolveWalk(string walk, JsonObject record)
        {
            if (string.IsNullOrWhiteSpace(walk))
            {
                return record.DeepClone();
            }

            var segments = walk.Split('/', StringSplitOptions.None);
            return ResolveWalk(segments, 0, record);
        }

        private JsonNode? ResolveWalk(string[] segments, int index, JsonNode? currentNode)
        {
            if (currentNode is null)
            {
                return null;
            }

            if (index >= segments.Length)
            {
                return currentNode.DeepClone();
            }

            var segment = segments[index];
            if (string.IsNullOrEmpty(segment))
            {
                return currentNode.DeepClone();
            }

            if (segment.Contains("(:", StringComparison.Ordinal))
            {
                return ResolveRelationshipSegment(segment, segments, index, currentNode);
            }

            var isArray = segment.EndsWith("[]", StringComparison.Ordinal);
            var cleanSegment = isArray ? segment[..^2] : segment;

            if (currentNode is not JsonObject currentObject)
            {
                return null;
            }

            var nextNode = currentObject[cleanSegment];
            if (!isArray)
            {
                return ResolveWalk(segments, index + 1, nextNode);
            }

            if (nextNode is not JsonArray nextArray)
            {
                return null;
            }

            if (index == segments.Length - 1)
            {
                return nextArray.DeepClone();
            }

            var results = new JsonArray();
            foreach (var item in nextArray)
            {
                AppendResolvedValue(results, ResolveWalk(segments, index + 1, item));
            }

            return results;
        }

        private JsonNode? ResolveRelationshipSegment(string segment, string[] segments, int index, JsonNode currentNode)
        {
            if (currentNode is not JsonObject currentObject)
            {
                return null;
            }

            var markerIndex = segment.IndexOf("(:", StringComparison.Ordinal);
            var propertyName = segment[..markerIndex];
            var typeName = segment[(markerIndex + 2)..].TrimEnd(')');

            var relationshipId = DataIOFileStore.GetStringValue(currentObject[propertyName]);
            if (string.IsNullOrWhiteSpace(relationshipId))
            {
                return null;
            }

            var relatedRecord = _fileStore.FindRelatedRecord(typeName, relationshipId);
            return ResolveWalk(segments, index + 1, relatedRecord);
        }

        private static void AppendResolvedValue(JsonArray target, JsonNode? resolvedValue)
        {
            if (resolvedValue is null)
            {
                return;
            }

            if (resolvedValue is JsonArray arrayValue)
            {
                foreach (var item in arrayValue)
                {
                    target.Add(item?.DeepClone());
                }

                return;
            }

            target.Add(resolvedValue.DeepClone());
        }
    }
}