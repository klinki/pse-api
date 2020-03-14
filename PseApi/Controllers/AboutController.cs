using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PseApi.Controllers.Dto;
using PseApi.Services;

namespace PseApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AboutController : ControllerBase
    {
        private readonly AppInfoService _appInfoService;

        public AboutController(AppInfoService appInfoService)
        {
            _appInfoService = appInfoService;
        }


        // GET api/values
        [HttpGet("version")]
        public async Task<ActionResult<VersionDto>> GetVersion()
        {
            return Ok(new
            {
                _appInfoService.Version,
                _appInfoService.BuildDate
            });
        }
    }
}
