using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PseApi.Data;

namespace PseApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DaysController : ControllerBase
    {
        private readonly PseContext _context;
        private readonly ILogger<DaysController> _logger;

        public DaysController(PseContext context, ILogger<DaysController> logger)
        {
            _context = context;
            _logger = logger;
        }


        // GET api/values
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<Dataset>>> GetAllDays()
        {
            IEnumerable<Dataset> result = await _context.Datasets.ToListAsync();

            return Ok(result.Select(dataset => dataset.Day.ToString("yyyy-MM-dd")));
        }
    }
}
