using ExtensionAppDataIO.Models;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json.Nodes;
using Metamodel = ExtensionAppDataIO.Models.Metamodel;

namespace ExtensionAppDataIO.Services
{
    public class DataIOService : IDataIOService
    {
        private readonly DataIODateTimeNormalizer _dateTimeNormalizer;
        private readonly DataIOFileStore _fileStore;
        private readonly IModelManagerService _modelManagerService;
        private readonly DataIOQueryExecutor _queryExecutor;
        private readonly DataIOResultRehydrator _resultRehydrator;

        public DataIOService(
            IWebHostEnvironment environment,
            IOptions<DataIOSettings> options,
            DataIODateTimeNormalizer dateTimeNormalizer,
            IModelManagerService modelManagerService)
        {
            var mockDbPath = options.Value.MockDbPath;
            var resolvedMockDbPath = Path.IsPathRooted(mockDbPath)
                ? mockDbPath
                : Path.Combine(environment.ContentRootPath, mockDbPath);

            _dateTimeNormalizer = dateTimeNormalizer;
            _fileStore = new DataIOFileStore(resolvedMockDbPath);
            _modelManagerService = modelManagerService;

            var pathResolver = new DataIOPathResolver(_fileStore);
            _queryExecutor = new DataIOQueryExecutor(pathResolver);
            _resultRehydrator = new DataIOResultRehydrator(pathResolver);
        }

        public Task<CreateRecordResponse> CreateRecord(CreateRecordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.typeName) || request.data is null)
            {
                throw new ArgumentException("typeName and data are required.");
            }

            var nextRecordId = _fileStore.ReadRecords(request.typeName).Count.ToString(CultureInfo.InvariantCulture);
            var record = (JsonObject)request.data.DeepClone();
            record["Id"] = nextRecordId;
            _dateTimeNormalizer.NormalizeForWrite(record, request.typeName);

            _fileStore.AppendRecord(request.typeName, record);

            return Task.FromResult(new CreateRecordResponse
            {
                recordId = nextRecordId,
            });
        }

        public Task<Metamodel.GetTypeDefinitionsResponse> GetTypeDefinitions(Metamodel.GetTypeDefinitionsBody request)
        {
            if (request.TypeNames is null)
            {
                throw new ArgumentException("typeNames is required.");
            }

            var requestedTypeNames = request.TypeNames
                .Select(typeName => typeName.TypeName)
                .Where(typeName => !string.IsNullOrWhiteSpace(typeName));

            return Task.FromResult(_modelManagerService.GetTypeDefinitions(requestedTypeNames));
        }

        public Task<Metamodel.GetTypeNamesResponse> GetTypeNames()
        {
            return Task.FromResult(_modelManagerService.GetTypeNames());
        }

        public Task<PatchRecordResponse> PatchRecord(PatchRecordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.typeName) || string.IsNullOrWhiteSpace(request.recordId) || request.data is null)
            {
                throw new ArgumentException("typeName, recordId, and data are required.");
            }

            var patchData = (JsonObject)request.data.DeepClone();
            _dateTimeNormalizer.NormalizeForWrite(patchData, request.typeName);

            var updated = _fileStore.PatchRecord(request.typeName, request.recordId, patchData);
            if (!updated)
            {
                throw new KeyNotFoundException($"Record '{request.recordId}' was not found.");
            }

            return Task.FromResult(new PatchRecordResponse
            {
                success = true,
            });
        }

        public Task<SearchRecordsResponse> SearchRecords(SearchRecordsRequest request)
        {
            if (request.query is null || request.pagination is null)
            {
                throw new ArgumentException("query and pagination are required.");
            }

            if (string.IsNullOrWhiteSpace(request.query.from))
            {
                throw new ArgumentException("query.from is required.");
            }

            var records = _fileStore.ReadRecords(request.query.from);
            var resultIndex = _queryExecutor.Execute(request.query, records);
            if (resultIndex < 0)
            {
                return Task.FromResult(new SearchRecordsResponse());
            }

            var projectedRecord = _resultRehydrator.FilterAndRehydrate(request.query.attributesToSelect, records[resultIndex]);
            _dateTimeNormalizer.NormalizeForRead(projectedRecord, request.query.from);

            return Task.FromResult(new SearchRecordsResponse
            {
                records = new List<JsonObject> { projectedRecord },
            });
        }
    }
}
