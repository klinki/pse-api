using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PseApi.Data;

namespace PseApi.Controllers
{
    [EnableCors]
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly PseContext _context;

        public StocksController(PseContext context)
        {
            _context = context;
        }

        // GET api/stock
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Stock>>> GetStocks()
        {
            IEnumerable<Stock> result = await _context.Stocks.AsNoTracking().ToListAsync();

            return Ok(result);
        }
    }
}
