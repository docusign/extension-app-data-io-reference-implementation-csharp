using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ExtensionAppDataIO.Services
{
    public class DataIOResultRehydrator
    {
        private readonly DataIOPathResolver _pathResolver;

        public DataIOResultRehydrator(DataIOPathResolver pathResolver)
        {
            _pathResolver = pathResolver;
        }

        public JsonObject FilterAndRehydrate(IEnumerable<string> attributesToSelect, JsonObject record)
        {
            var rehydratedRecord = new JsonObject();

            foreach (var attribute in attributesToSelect)
            {
                var value = _pathResolver.ResolveWalk(attribute, record);
                if (value is not null)
                {
                    AssignValueByPath(rehydratedRecord, attribute, value);
                }
            }

            return rehydratedRecord;
        }

        private static void AssignValueByPath(JsonObject target, string path, JsonNode value)
        {
            var segments = path.Split('/', StringSplitOptions.None);
            var current = target;

            for (var index = 0; index < segments.Length; index++)
            {
                var segment = NormalizeSegment(segments[index]);

                if (index == segments.Length - 1)
                {
                    current[segment] = value.DeepClone();
                    continue;
                }

                if (current[segment] is not JsonObject nextObject)
                {
                    nextObject = new JsonObject();
                    current[segment] = nextObject;
                }

                current = nextObject;
            }
        }

        private static string NormalizeSegment(string segment)
        {
            return Regex.Replace(segment, @"\(:.*?\)", string.Empty).Replace("[]", string.Empty);
        }
    }
}