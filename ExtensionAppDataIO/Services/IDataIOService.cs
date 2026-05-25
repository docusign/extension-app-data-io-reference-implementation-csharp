using ExtensionAppDataIO.Models;

namespace ExtensionAppDataIO.Services
{
    public interface IDataIOService
    {
        Task<CreateRecordResponse> CreateRecord(CreateRecordRequest request);
        Task<PatchRecordResponse> PatchRecord(PatchRecordRequest request);
        Task<SearchRecordsResponse> SearchRecords(SearchRecordsRequest request);
        Task<GetTypeNamesResponse> GetTypeNames();
        Task<GetTypeDefinitionsResponse> GetTypeDefinitions(GetTypeDefinitionsRequest request);
    }
}