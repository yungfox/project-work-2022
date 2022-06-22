using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashBoard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParkingController : ControllerBase
    {
        private readonly ILogger<ParkingController> _logger;

        public ParkingController(ILogger<ParkingController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public string Get()
        {
            return "yay";
        }

        [HttpPost]
        public string Post()
        {
            return "post";
        }
    }
}
