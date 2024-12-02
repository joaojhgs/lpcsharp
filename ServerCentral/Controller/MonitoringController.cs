using Microsoft.AspNetCore.Mvc;
using ServerCentral.Data;
using ServerCentral.Models;
using System.Linq;

namespace ServerCentral.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonitoringController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MonitoringController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult PostMonitoringData([FromBody] MonitoringData data)
        {
            if (data == null)
            {
                return BadRequest("Dados inválidos.");
            }

            _context.MonitoringData.Add(data);
            _context.SaveChanges();
            return Ok("Dados recebidos com sucesso.");
        }

        [HttpGet]
        public IActionResult GetMonitoringData()
        {
            var data = _context.MonitoringData.OrderByDescending(d => d.Timestamp).ToList();
            return Ok(data);
        }
    }
}
