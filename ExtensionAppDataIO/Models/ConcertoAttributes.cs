namespace ExtensionAppDataIO.Models
{
    /// <summary>Mirrors the @Term("label") CTO decorator.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class TermAttribute : Attribute
    {
        public string Label { get; }
        public TermAttribute(string label) => Label = label;
    }

    /// <summary>Mirrors the @Crud("permissions") CTO decorator.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class CrudAttribute : Attribute
    {
        public string Permissions { get; }
        public CrudAttribute(string permissions) => Permissions = permissions;
    }

    /// <summary>Marks a property that maps to a CTO relationship (-->).</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class RelationshipAttribute : Attribute { }

    /// <summary>Marks a property as optional in generated metamodel output.</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class OptionalAttribute : Attribute { }

    /// <summary>Marks the property that is the concept identifier (identified by ...).</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ConcertoIdentifierAttribute : Attribute { }
}
