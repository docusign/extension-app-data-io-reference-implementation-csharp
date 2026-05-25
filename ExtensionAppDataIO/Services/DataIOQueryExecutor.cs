using System.Globalization;
using System.Text.Json.Nodes;
using ExtensionAppDataIO.Models;

namespace ExtensionAppDataIO.Services
{
    public class DataIOQueryExecutor
    {
        private readonly DataIOPathResolver _pathResolver;

        public DataIOQueryExecutor(DataIOPathResolver pathResolver)
        {
            _pathResolver = pathResolver;
        }

        public int Execute(QueryDefinition query, IReadOnlyList<JsonObject> inputData)
        {
            var operation = query.queryFilter?.operation;
            if (operation is null)
            {
                return -1;
            }

            for (var index = 0; index < inputData.Count; index++)
            {
                if (EvaluateOperation(operation, inputData[index]))
                {
                    return index;
                }
            }

            return -1;
        }

        private bool EvaluateOperation(QueryOperation operation, JsonObject record)
        {
            if (operation.leftOperand is not null || operation.rightOperand is not null)
            {
                return ExecuteComparison(operation, record);
            }

            if (operation.leftOperation is not null && operation.rightOperation is not null)
            {
                return ExecuteLogical(operation, record);
            }

            return false;
        }

        private bool ExecuteComparison(QueryOperation operation, JsonObject record)
        {
            if (operation.leftOperand is null || operation.rightOperand is null)
            {
                return false;
            }

            var leftValue = ResolveOperand(operation.leftOperand, record);
            var rightValue = ResolveOperand(operation.rightOperand, record);

            return operation.@operator switch
            {
                "EQUALS" => DeepEqual(leftValue, rightValue),
                "NOT_EQUALS" => !DeepEqual(leftValue, rightValue),
                "GREATER_THAN" => CompareValues(leftValue, rightValue) > 0,
                "LESS_THAN" => CompareValues(leftValue, rightValue) < 0,
                "GREATER_THAN_OR_EQUALS_TO" => CompareValues(leftValue, rightValue) >= 0,
                "LESS_THAN_OR_EQUALS_TO" => CompareValues(leftValue, rightValue) <= 0,
                "IN" => rightValue is List<object?> rightList && rightList.Any(item => DeepEqual(leftValue, item)),
                _ => false,
            };
        }

        private bool ExecuteLogical(QueryOperation operation, JsonObject record)
        {
            if (operation.leftOperation is null || operation.rightOperation is null)
            {
                return false;
            }

            var leftResult = EvaluateOperation(operation.leftOperation, record);
            var rightResult = EvaluateOperation(operation.rightOperation, record);

            return operation.@operator switch
            {
                "AND" => leftResult && rightResult,
                "OR" => leftResult || rightResult,
                _ => false,
            };
        }

        private object? ResolveOperand(QueryOperand operand, JsonObject record)
        {
            if (operand.isLiteral)
            {
                return CoerceScalar(operand.name, operand.type);
            }

            return CoerceNode(_pathResolver.ResolveWalk(operand.name, record), operand.type);
        }

        private static object? CoerceNode(JsonNode? node, string operandType)
        {
            if (node is null)
            {
                return null;
            }

            if (node is JsonArray arrayNode)
            {
                return arrayNode.Select(item => CoerceNode(item, operandType)).ToList();
            }

            if (node is JsonObject objectNode)
            {
                return objectNode.DeepClone();
            }

            var scalarValue = DataIOFileStore.GetStringValue(node);
            return scalarValue is null ? null : CoerceScalar(scalarValue, operandType);
        }

        private static object CoerceScalar(string value, string operandType)
        {
            return operandType switch
            {
                "INTEGER" when int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integerValue) => integerValue,
                "LONG" when long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue) => longValue,
                "DOUBLE" when double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var doubleValue) => doubleValue,
                "BOOLEAN" when bool.TryParse(value, out var booleanValue) => booleanValue,
                "DATETIME" when DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateValue) => dateValue.ToUnixTimeMilliseconds(),
                _ => value,
            };
        }

        private static bool DeepEqual(object? leftValue, object? rightValue)
        {
            if (leftValue is null || rightValue is null)
            {
                return leftValue is null && rightValue is null;
            }

            if (leftValue is JsonNode leftNode && rightValue is JsonNode rightNode)
            {
                return JsonNode.DeepEquals(leftNode, rightNode);
            }

            if (leftValue is List<object?> leftList && rightValue is List<object?> rightList)
            {
                if (leftList.Count != rightList.Count)
                {
                    return false;
                }

                for (var index = 0; index < leftList.Count; index++)
                {
                    if (!DeepEqual(leftList[index], rightList[index]))
                    {
                        return false;
                    }
                }

                return true;
            }

            if (IsNumeric(leftValue) && IsNumeric(rightValue))
            {
                return Convert.ToDouble(leftValue, CultureInfo.InvariantCulture) == Convert.ToDouble(rightValue, CultureInfo.InvariantCulture);
            }

            return string.Equals(
                Convert.ToString(leftValue, CultureInfo.InvariantCulture),
                Convert.ToString(rightValue, CultureInfo.InvariantCulture),
                StringComparison.Ordinal);
        }

        private static int CompareValues(object? leftValue, object? rightValue)
        {
            if (leftValue is null || rightValue is null)
            {
                if (leftValue is null && rightValue is null)
                {
                    return 0;
                }

                return leftValue is null ? -1 : 1;
            }

            if (IsNumeric(leftValue) && IsNumeric(rightValue))
            {
                return Convert.ToDouble(leftValue, CultureInfo.InvariantCulture)
                    .CompareTo(Convert.ToDouble(rightValue, CultureInfo.InvariantCulture));
            }

            return string.CompareOrdinal(
                Convert.ToString(leftValue, CultureInfo.InvariantCulture),
                Convert.ToString(rightValue, CultureInfo.InvariantCulture));
        }

        private static bool IsNumeric(object value)
        {
            return value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal;
        }
    }
}