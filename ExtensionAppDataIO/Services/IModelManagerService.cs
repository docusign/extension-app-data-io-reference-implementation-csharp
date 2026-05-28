using Metamodel = ExtensionAppDataIO.Models.Metamodel;

namespace ExtensionAppDataIO.Services
{
    public interface IModelManagerService
    {
        Metamodel.GetTypeNamesResponse GetTypeNames();
        Metamodel.GetTypeDefinitionsResponse GetTypeDefinitions(IEnumerable<string>? requestedTypeNames);
    }
}
