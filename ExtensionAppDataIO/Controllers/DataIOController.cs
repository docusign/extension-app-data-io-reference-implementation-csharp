using ExtensionAppDataIO.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExtensionAppDataIO.Controllers
{
    public class DataIOController : Controller
    {
        private IDataIOService _dataIOService;

        public DataIOController(IDataIOService dataIOService)
        {
            _dataIOService = dataIOService;
        }

        public async Task<IActionResult> CreateRecord()
        {
            await _dataIOService.CreateRecord();
            return View();
        }

        public async Task<IActionResult> PatchRecord()
        {
            await _dataIOService.PatchRecord();
            return View();
        }

        public async Task<IActionResult> SearchRecords()
        {
            await _dataIOService.SearchRecords();
            return View();
        }

        public async Task<IActionResult> GetTypeNames()
        {
            await _dataIOService.GetTypeNames();
            return View();
        }

        public async Task<IActionResult> GetTypeDefinitions()
        {
            await _dataIOService.GetTypeDefinitions();
            return View();
        }
    }
}
