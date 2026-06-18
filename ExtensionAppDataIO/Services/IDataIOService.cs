using ExtensionAppDataIO.Models;
using Metamodel = ExtensionAppDataIO.Models.Metamodel;

namespace ExtensionAppDataIO.Services
{
    public interface IDataIOService
    {
        Task<CreateRecordResponse> CreateRecord(CreateRecordRequest request);
        Task<PatchRecordResponse> PatchRecord(PatchRecordRequest request);
        Task<SearchRecordsResponse> SearchRecords(SearchRecordsRequest request);
        Task<Metamodel.GetTypeNamesResponse> GetTypeNames();
        Task<Metamodel.GetTypeDefinitionsResponse> GetTypeDefinitions(Metamodel.GetTypeDefinitionRequestBody request);
    }
}