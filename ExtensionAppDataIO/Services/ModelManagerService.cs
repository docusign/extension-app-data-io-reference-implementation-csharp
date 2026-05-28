using ExtensionAppDataIO.Models;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;
using Metamodel = ExtensionAppDataIO.Models.Metamodel;

namespace ExtensionAppDataIO.Services
{
    public class ModelManagerService : IModelManagerService
    {
        private static readonly string DataModelsNamespace = "ExtensionAppDataIO.DataModels";
        private static readonly Assembly Asm = typeof(ModelManagerService).Assembly;

        // ── Type discovery ──────────────────────────────────────────────────

        private static IEnumerable<Type> GetConceptTypes() =>
            Asm.GetTypes()
               .Where(t => t.Namespace == DataModelsNamespace
                        && t.IsClass
                        && !t.IsAbstract
                        && typeof(Concept).IsAssignableFrom(t));

        private static IEnumerable<Type> GetEnumTypes() =>
            Asm.GetTypes()
               .Where(t => t.Namespace == DataModelsNamespace && t.IsEnum);

        // ── Public API ──────────────────────────────────────────────────────

        public Metamodel.GetTypeNamesResponse GetTypeNames()
        {
            var typeNames = GetConceptTypes()
                .Where(IsReadable)
                .Select(t => new Metamodel.TypeNameInfo
                {
                    TypeName = t.Name,
                    Label = t.GetCustomAttribute<TermAttribute>()?.Label ?? t.Name,
                })
                .ToList();

            return new Metamodel.GetTypeNamesResponse { TypeNames = typeNames };
        }

        public Metamodel.GetTypeDefinitionsResponse GetTypeDefinitions(IEnumerable<string>? requestedTypeNames)
        {
            var declarations = new List<Metamodel.MetamodelDeclaration>();

            foreach (var enumType in GetEnumTypes())
            {
                declarations.Add(BuildEnumDeclaration(enumType));
            }

            foreach (var conceptType in GetConceptTypes())
            {
                declarations.Add(BuildConceptDeclaration(conceptType));
            }

            return new Metamodel.GetTypeDefinitionsResponse
            {
                Declarations = declarations,
            };
        }

        // ── Builders ────────────────────────────────────────────────────────

        private static bool IsReadable(Type t) =>
            t.GetCustomAttribute<CrudAttribute>()?.Permissions.Contains("Readable") == true;

        private static Metamodel.MetamodelEnumDeclaration BuildEnumDeclaration(Type enumType)
        {
            var properties = Enum.GetNames(enumType)
                .Select(name => new Metamodel.MetamodelEnumValueProperty
                {
                    Name = GetEnumMemberValue(enumType, name),
                })
                .ToList();

            return new Metamodel.MetamodelEnumDeclaration
            {
                Name = ToMetamodelTypeName(enumType),
                Properties = properties,
            };
        }

        private static Metamodel.MetamodelConceptDeclaration BuildConceptDeclaration(Type conceptType)
        {
            var decorators = BuildClassDecorators(conceptType);

            var identifierProp = conceptType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .FirstOrDefault(p => p.GetCustomAttribute<ConcertoIdentifierAttribute>() != null);

            var properties = conceptType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(p => p.Name != "_Class")
                .Select(BuildProperty)
                .ToList();

            return new Metamodel.MetamodelConceptDeclaration
            {
                Name = conceptType.Name,
                IsAbstract = false,
                Identified = identifierProp is not null
                    ? new Metamodel.MetamodelIdentifiedBy { Name = GetCtoName(identifierProp) }
                    : null,
                Decorators = decorators.Count > 0 ? decorators : null,
                Properties = properties,
            };
        }

        private static List<Metamodel.MetamodelDecorator> BuildClassDecorators(Type t)
        {
            var result = new List<Metamodel.MetamodelDecorator>();
            var term = t.GetCustomAttribute<TermAttribute>();
            var crud = t.GetCustomAttribute<CrudAttribute>();
            if (term is not null) result.Add(MakeDecorator("Term", term.Label));
            if (crud is not null) result.Add(MakeDecorator("Crud", crud.Permissions));
            return result;
        }

        private static Metamodel.MetamodelProperty BuildProperty(PropertyInfo prop)
        {
            var term = prop.GetCustomAttribute<TermAttribute>();
            var crud = prop.GetCustomAttribute<CrudAttribute>();
            var isRelationship = prop.GetCustomAttribute<RelationshipAttribute>() is not null;

            var decorators = new List<Metamodel.MetamodelDecorator>();
            if (term is not null) decorators.Add(MakeDecorator("Term", term.Label));
            if (crud is not null) decorators.Add(MakeDecorator("Crud", crud.Permissions));

            var (baseType, isArray, isOptional) = UnwrapType(prop.PropertyType);

            Metamodel.MetamodelProperty mp = isRelationship
                ? new Metamodel.MetamodelRelationshipProperty { Type = new Metamodel.TypeIdentifier { Name = BuildTypeIdentifierName(baseType) } }
                : MapToMetamodelProperty(baseType);

            mp.Name = GetCtoName(prop);
            mp.IsArray = isArray;
            mp.IsOptional = isOptional;
            mp.Decorators = decorators.Count > 0 ? decorators : null;

            if (mp is Metamodel.MetamodelStringProperty stringProperty)
            {
                var maxLength = prop.GetCustomAttribute<MaxLengthAttribute>()?.Length;
                if (maxLength is not null)
                {
                    stringProperty.LengthValidator = new Metamodel.MetamodelStringLengthValidator
                    {
                        MinLength = 0,
                        MaxLength = maxLength.Value,
                    };
                }
            }

            return mp;
        }

        private static Metamodel.MetamodelProperty MapToMetamodelProperty(Type baseType)
        {
            if (baseType == typeof(string))   return new Metamodel.MetamodelStringProperty();
            if (baseType == typeof(float) || baseType == typeof(double)) return new Metamodel.MetamodelDoubleProperty();
            if (baseType == typeof(int))      return new Metamodel.MetamodelIntegerProperty();
            if (baseType == typeof(bool))     return new Metamodel.MetamodelBooleanProperty();
            if (baseType == typeof(DateTime)) return new Metamodel.MetamodelDateTimeProperty();

            if (baseType.IsEnum || typeof(Concept).IsAssignableFrom(baseType))
                return new Metamodel.MetamodelObjectProperty { Type = new Metamodel.TypeIdentifier { Name = BuildTypeIdentifierName(baseType) } };

            return new Metamodel.MetamodelStringProperty(); // fallback
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private static (Type baseType, bool isArray, bool isOptional) UnwrapType(Type type)
        {
            bool isOptional = false;
            bool isArray = false;

            var nullable = Nullable.GetUnderlyingType(type);
            if (nullable is not null) { isOptional = true; type = nullable; }

            if (type.IsArray)
            {
                isArray = true;
                type = type.GetElementType()!;
                var innerNullable = Nullable.GetUnderlyingType(type);
                if (innerNullable is not null) type = innerNullable;
            }

            // Reference types are inherently nullable
            if (!type.IsValueType) isOptional = true;

            return (type, isArray, isOptional);
        }

        private static string GetCtoName(PropertyInfo prop)
        {
            var jsonAttr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
            return jsonAttr?.Name ?? prop.Name;
        }

        private static string GetEnumMemberValue(Type enumType, string name)
        {
            var member = enumType.GetMember(name).FirstOrDefault();
            var attr = member?.GetCustomAttribute<System.Runtime.Serialization.EnumMemberAttribute>();
            return attr?.Value ?? name;
        }

        private static string BuildTypeIdentifierName(Type type) =>
            type.IsEnum ? ToMetamodelTypeName(type) : type.Name;

        private static string ToMetamodelTypeName(Type type)
        {
            if (type.IsEnum && type.Name.EndsWith("Enum", StringComparison.Ordinal))
                return $"{type.Name[..^4]}_Enum";

            return type.Name;
        }

        private static Metamodel.MetamodelDecorator MakeDecorator(string name, string value) =>
            new Metamodel.MetamodelDecorator
            {
                Name = name,
                Arguments = new List<Metamodel.DecoratorArgument> { new Metamodel.DecoratorString { Value = value } },
            };
    }
}
