using System.Text.Json.Serialization;

namespace ExtensionAppDataIO.Models
{
    // ── Decorator ────────────────────────────────────────────────────────────

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$class")]
    [JsonDerivedType(typeof(DecoratorString), "concerto.metamodel@1.0.0.DecoratorString")]
    public abstract class DecoratorArgument { }

    public class DecoratorString : DecoratorArgument
    {
        public string Value { get; set; } = string.Empty;
    }

    public class MetamodelDecorator
    {
        [JsonPropertyName("$class")]
        public string Class => "concerto.metamodel@1.0.0.Decorator";

        public string Name { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<DecoratorArgument>? Arguments { get; set; }
    }

    // ── Type reference ────────────────────────────────────────────────────────

    public class TypeIdentifier
    {
        [JsonPropertyName("$class")]
        public string Class => "concerto.metamodel@1.0.0.TypeIdentifier";

        public string Name { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Namespace { get; set; }
    }

    // ── IdentifiedBy ──────────────────────────────────────────────────────────

    public class MetamodelIdentifiedBy
    {
        [JsonPropertyName("$class")]
        public string Class => "concerto.metamodel@1.0.0.IdentifiedBy";

        public string Name { get; set; } = string.Empty;
    }

    // ── Properties ────────────────────────────────────────────────────────────

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$class")]
    [JsonDerivedType(typeof(MetamodelStringProperty),   "concerto.metamodel@1.0.0.StringProperty")]
    [JsonDerivedType(typeof(MetamodelDoubleProperty),   "concerto.metamodel@1.0.0.DoubleProperty")]
    [JsonDerivedType(typeof(MetamodelIntegerProperty),  "concerto.metamodel@1.0.0.IntegerProperty")]
    [JsonDerivedType(typeof(MetamodelBooleanProperty),  "concerto.metamodel@1.0.0.BooleanProperty")]
    [JsonDerivedType(typeof(MetamodelDateTimeProperty), "concerto.metamodel@1.0.0.DateTimeProperty")]
    [JsonDerivedType(typeof(MetamodelObjectProperty),   "concerto.metamodel@1.0.0.ObjectProperty")]
    [JsonDerivedType(typeof(MetamodelRelationshipProperty), "concerto.metamodel@1.0.0.RelationshipProperty")]
    public abstract class MetamodelProperty
    {
        public string Name { get; set; } = string.Empty;
        public bool IsArray { get; set; }
        public bool IsOptional { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<MetamodelDecorator>? Decorators { get; set; }
    }

    // Primitive properties (no extra fields beyond the base)
    public class MetamodelStringProperty : MetamodelProperty
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MetamodelStringLengthValidator? LengthValidator { get; set; }
    }

    public class MetamodelStringLengthValidator
    {
        [JsonPropertyName("$class")]
        public string Class => "concerto.metamodel@1.0.0.StringLengthValidator";

        public int MinLength { get; set; }
        public int MaxLength { get; set; }
    }

    public class MetamodelDoubleProperty   : MetamodelProperty { }
    public class MetamodelIntegerProperty  : MetamodelProperty { }
    public class MetamodelBooleanProperty  : MetamodelProperty { }
    public class MetamodelDateTimeProperty : MetamodelProperty { }

    // Object / relationship properties carry a type reference
    public class MetamodelObjectProperty : MetamodelProperty
    {
        public TypeIdentifier Type { get; set; } = new();
    }

    public class MetamodelRelationshipProperty : MetamodelProperty
    {
        public TypeIdentifier Type { get; set; } = new();
    }

    // Enum value (inside an EnumDeclaration)
    public class MetamodelEnumValueProperty
    {
        [JsonPropertyName("$class")]
        public string Class => "concerto.metamodel@1.0.0.EnumProperty";

        public string Name { get; set; } = string.Empty;
    }

    // ── Declarations ─────────────────────────────────────────────────────────

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$class")]
    [JsonDerivedType(typeof(MetamodelConceptDeclaration), "concerto.metamodel@1.0.0.ConceptDeclaration")]
    [JsonDerivedType(typeof(MetamodelEnumDeclaration),    "concerto.metamodel@1.0.0.EnumDeclaration")]
    public abstract class MetamodelDeclaration
    {
        public string Name { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<MetamodelDecorator>? Decorators { get; set; }
    }

    public class MetamodelConceptDeclaration : MetamodelDeclaration
    {
        public bool IsAbstract { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MetamodelIdentifiedBy? Identified { get; set; }

        public List<MetamodelProperty> Properties { get; set; } = new();
    }

    public class MetamodelEnumDeclaration : MetamodelDeclaration
    {
        public List<MetamodelEnumValueProperty> Properties { get; set; } = new();
    }

    // ── Service response models ───────────────────────────────────────────────

    public class TypeNameInfo
    {
        public string TypeName { get; set; } = string.Empty;
        public string Label    { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }
    }

    public class GetTypeNamesResponse
    {
        public List<TypeNameInfo> TypeNames { get; set; } = new();
    }

    public class GetTypeDefinitionsBody
    {
        public List<TypeNameInfo>? TypeNames { get; set; }
    }

    public class GetTypeDefinitionRequestBody
    {
        public string[] TypeNames { get; set; } = Array.Empty<string>();
    }

    public class GetTypeDefinitionsResponse
    {
        public List<MetamodelDeclaration> Declarations { get; set; } = new();

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<GetTypeDefinitionsError>? Errors { get; set; }
    }

    public class GetTypeDefinitionsError
    {
        public string TypeName { get; set; } = string.Empty;
        public string Code     { get; set; } = string.Empty;
        public string Message  { get; set; } = string.Empty;
    }
}
