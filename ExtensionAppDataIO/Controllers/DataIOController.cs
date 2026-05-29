using ExtensionAppDataIO.Models;
using ExtensionAppDataIO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Metamodel = ExtensionAppDataIO.Models.Metamodel;

namespace ExtensionAppDataIO.Controllers
{
    [Route("api/dataio")]
    public class DataIOController : Controller
    {
        private readonly IDataIOService _dataIOService;

        public DataIOController(IDataIOService dataIOService)
        {
            _dataIOService = dataIOService;
        }

        [HttpPost("createRecord")]
        [Authorize]
        public async Task<IActionResult> CreateRecord([FromBody] CreateRecordRequest request)
        {
            return await Execute(() => _dataIOService.CreateRecord(request));
        }

        [HttpPost("patchRecord")]
        [Authorize]
        public async Task<IActionResult> PatchRecord([FromBody] PatchRecordRequest request)
        {
            return await Execute(() => _dataIOService.PatchRecord(request));
        }

        [HttpPost("searchRecords")]
        [Authorize]
        public async Task<IActionResult> SearchRecords([FromBody] SearchRecordsRequest request)
        {
            return await Execute(() => _dataIOService.SearchRecords(request));
        }

        [HttpPost("getTypeNames")]
        [Authorize]
        public async Task<IActionResult> GetTypeNames()
        {
            return await Execute(() => _dataIOService.GetTypeNames());
        }

        [HttpPost("getTypeDefinitions")]
        [Authorize]
        public async Task<IActionResult> GetTypeDefinitions([FromBody] Metamodel.GetTypeDefinitionRequestBody request)
        {
            return await Execute(() => _dataIOService.GetTypeDefinitions(request));
        }

        private async Task<IActionResult> Execute<T>(Func<Task<T>> action)
        {
            try
            {
                return Ok(await action());
            }
            catch (ArgumentException exception)
            {
                return BadRequest(CreateErrorResponse("BAD_REQUEST", exception.Message));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(CreateErrorResponse("NOT_FOUND", exception.Message));
            }
            catch
            {
                return StatusCode(500, CreateErrorResponse("INTERNAL_ERROR", "An internal error occurred."));
            }
        }

        private static object CreateErrorResponse(string code, string message)
        {
            return new { code, message };
        }
    }
}
