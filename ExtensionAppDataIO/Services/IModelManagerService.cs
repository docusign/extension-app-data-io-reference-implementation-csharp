using ExtensionAppDataIO.Models;

namespace ExtensionAppDataIO.Services
{
    public interface IModelManagerService
    {
        GetTypeNamesResponse GetTypeNames();
        GetTypeDefinitionsResponse GetTypeDefinitions(IEnumerable<string>? requestedTypeNames);
    }
}
