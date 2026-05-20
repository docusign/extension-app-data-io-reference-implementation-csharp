using ExtensionAppDataIO.Models;

namespace ExtensionAppDataIO.Services
{
    public interface IDataIOService
    {
        public Task<Record> CreateRecord();
        public Task<Record> PatchRecord();
        public Task<Record> SearchRecords();
        public Task GetTypeNames();
        public Task GetTypeDefinitions();
    }
}
