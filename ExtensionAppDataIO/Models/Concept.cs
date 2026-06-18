using System.Text.Json.Serialization;

namespace ExtensionAppDataIO.Models {
    public abstract class Concept {
       [JsonPropertyName("$class")]
       public abstract string _Class { get; }
    }
}