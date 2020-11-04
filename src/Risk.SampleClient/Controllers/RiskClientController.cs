using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Risk.SampleClient.Controllers
{
    public class RiskClientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("[action]")]
        public string AreYouThere()
        {
            return "yes";
        }
    }
}
