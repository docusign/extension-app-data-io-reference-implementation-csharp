using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Metamodel = ExtensionAppDataIO.Models.Metamodel;

namespace ExtensionAppDataIO.Services
{
    public class DataIODateTimeNormalizer
    {
        private static readonly Regex ValidRegex = new(
            "^(\\d{4}-\\d{2}-\\d{2}(T\\d{2}:\\d{2}:\\d{2}(\\.\\d{1,3})?(Z|[+-]\\d{2}:\\d{2})?)?)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly IReadOnlyDictionary<string, Metamodel.MetamodelConceptDeclaration> _conceptDeclarations;
        private readonly IReadOnlyDictionary<string, HashSet<string>> _enumDeclarations;

        public DataIODateTimeNormalizer(IModelManagerService modelManagerService)
        {
            var declarations = modelManagerService
                .GetTypeDefinitions(null)
                .Declarations;

            _conceptDeclarations = declarations
                .OfType<Metamodel.MetamodelConceptDeclaration>()
                .ToDictionary(declaration => declaration.Name, StringComparer.Ordinal);

            _enumDeclarations = declarations
                .OfType<Metamodel.MetamodelEnumDeclaration>()
                .ToDictionary(
                    declaration => declaration.Name,
                    declaration => declaration.Properties
                        .Select(property => property.Name)
                        .ToHashSet(StringComparer.Ordinal),
                    StringComparer.Ordinal);
        }

        public void NormalizeForWrite(JsonObject record, string typeName)
        {
            NormalizeObject(record, typeName, strict: true, parentPath: null);
        }

        public void NormalizeForRead(JsonObject record, string typeName)
        {
            NormalizeObject(record, typeName, strict: false, parentPath: null);
        }

        private void NormalizeObject(JsonObject record, string typeName, bool strict, string? parentPath)
        {
            if (!_conceptDeclarations.TryGetValue(typeName, out var declaration))
            {
                return;
            }

            foreach (var property in declaration.Properties)
            {
                if (!record.TryGetPropertyValue(property.Name, out var propertyNode) || propertyNode is null)
                {
                    continue;
                }

                var propertyPath = string.IsNullOrWhiteSpace(parentPath)
                    ? property.Name
                    : $"{parentPath}/{property.Name}";

                NormalizeProperty(propertyNode, property, strict, propertyPath);
            }
        }

        private void NormalizeProperty(JsonNode propertyNode, Metamodel.MetamodelProperty property, bool strict, string propertyPath)
        {
            switch (property)
            {
                case Metamodel.MetamodelDateTimeProperty:
                    NormalizeDateTimeNode(propertyNode, strict, propertyPath);
                    break;
                case Metamodel.MetamodelObjectProperty objectProperty:
                    if (TryValidateEnumNode(propertyNode, objectProperty.Type.Name, strict, propertyPath))
                    {
                        break;
                    }

                    NormalizeNestedNode(propertyNode, objectProperty.Type.Name, strict, propertyPath);
                    break;
                case Metamodel.MetamodelRelationshipProperty relationshipProperty:
                    NormalizeNestedNode(propertyNode, relationshipProperty.Type.Name, strict, propertyPath);
                    break;
            }
        }

        private bool TryValidateEnumNode(JsonNode propertyNode, string typeName, bool strict, string propertyPath)
        {
            if (!_enumDeclarations.TryGetValue(typeName, out var allowedValues))
            {
                return false;
            }

            ValidateEnumNode(propertyNode, allowedValues, strict, propertyPath);
            return true;
        }

        private void NormalizeNestedNode(JsonNode propertyNode, string typeName, bool strict, string propertyPath)
        {
            switch (propertyNode)
            {
                case JsonObject objectNode:
                    NormalizeObject(objectNode, typeName, strict, propertyPath);
                    break;
                case JsonArray arrayNode:
                    for (var index = 0; index < arrayNode.Count; index++)
                    {
                        if (arrayNode[index] is JsonObject arrayItem)
                        {
                            NormalizeObject(arrayItem, typeName, strict, $"{propertyPath}[{index}]");
                        }
                    }
                    break;
            }
        }

        private static void ValidateEnumNode(JsonNode propertyNode, HashSet<string> allowedValues, bool strict, string propertyPath)
        {
            switch (propertyNode)
            {
                case JsonValue valueNode:
                    ValidateEnumValue(valueNode, allowedValues, strict, propertyPath);
                    break;
                case JsonArray arrayNode:
                    for (var index = 0; index < arrayNode.Count; index++)
                    {
                        if (arrayNode[index] is not JsonValue arrayValue)
                        {
                            if (strict)
                            {
                                throw CreateEnumException($"{propertyPath}[{index}]", arrayNode[index], allowedValues);
                            }

                            continue;
                        }

                        ValidateEnumValue(arrayValue, allowedValues, strict, $"{propertyPath}[{index}]");
                    }
                    break;
                default:
                    if (strict)
                    {
                        throw CreateEnumException(propertyPath, propertyNode, allowedValues);
                    }
                    break;
            }
        }

        private static void NormalizeDateTimeNode(JsonNode propertyNode, bool strict, string propertyPath)
        {
            switch (propertyNode)
            {
                case JsonValue valueNode:
                    if (TryNormalizeDateTimeValue(valueNode, strict, propertyPath, out var normalizedValue))
                    {
                        valueNode.ReplaceWith(normalizedValue!);
                    }
                    break;
                case JsonArray arrayNode:
                    for (var index = 0; index < arrayNode.Count; index++)
                    {
                        if (arrayNode[index] is not JsonValue arrayValue)
                        {
                            if (strict)
                            {
                                throw CreateFormatException($"{propertyPath}[{index}]", arrayNode[index]);
                            }

                            continue;
                        }

                        if (TryNormalizeDateTimeValue(arrayValue, strict, $"{propertyPath}[{index}]", out var normalizedArrayValue))
                        {
                            arrayNode[index] = JsonValue.Create(normalizedArrayValue);
                        }
                    }
                    break;
                default:
                    if (strict)
                    {
                        throw CreateFormatException(propertyPath, propertyNode);
                    }
                    break;
            }
        }

        internal static bool TryNormalizeDateTimeString(string value, bool strict, string propertyPath, out string? normalizedValue)
        {
            normalizedValue = null;

            if (!ValidRegex.IsMatch(value) || !TryParseUtc(value, out var utcValue))
            {
                if (strict)
                {
                    throw CreateFormatException(propertyPath, value);
                }

                return false;
            }

            normalizedValue = utcValue.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
            return true;
        }

        private static bool TryNormalizeDateTimeValue(JsonValue valueNode, bool strict, string propertyPath, out string? normalizedValue)
        {
            normalizedValue = null;

            if (!valueNode.TryGetValue<string>(out var stringValue) || string.IsNullOrWhiteSpace(stringValue))
            {
                if (strict)
                {
                    throw CreateFormatException(propertyPath, valueNode);
                }

                return false;
            }

            return TryNormalizeDateTimeString(stringValue, strict, propertyPath, out normalizedValue);
        }

        private static void ValidateEnumValue(JsonValue valueNode, HashSet<string> allowedValues, bool strict, string propertyPath)
        {
            if (valueNode.TryGetValue<string>(out var stringValue)
                && !string.IsNullOrWhiteSpace(stringValue)
                && allowedValues.Contains(stringValue))
            {
                return;
            }

            if (strict)
            {
                throw CreateEnumException(propertyPath, valueNode, allowedValues);
            }
        }

        private static bool TryParseUtc(string value, out DateTimeOffset utcValue)
        {
            if (!value.Contains('T', StringComparison.Ordinal))
            {
                if (DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnly))
                {
                    utcValue = new DateTimeOffset(dateOnly.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
                    return true;
                }

                utcValue = default;
                return false;
            }

            return DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out utcValue);
        }

        private static FormatException CreateFormatException(string propertyPath, object? value)
        {
            var renderedValue = value switch
            {
                JsonNode node => DataIOFileStore.GetStringValue(node),
                _ => Convert.ToString(value, CultureInfo.InvariantCulture),
            };

            return new FormatException($"Invalid date format for property \"{propertyPath}\": \"{renderedValue}\". Must match a valid ISO 8601 format.");
        }

        private static ArgumentException CreateEnumException(string propertyPath, object? value, IEnumerable<string> allowedValues)
        {
            var renderedValue = value switch
            {
                JsonNode node => DataIOFileStore.GetStringValue(node),
                _ => Convert.ToString(value, CultureInfo.InvariantCulture),
            };

            var orderedValues = string.Join(", ", allowedValues.OrderBy(allowedValue => allowedValue, StringComparer.Ordinal));

            return new ArgumentException($"Invalid enum value for property \"{propertyPath}\": \"{renderedValue}\". Allowed values: {orderedValues}.");
        }
    }
}