using HVO.Hardware.RoofControllerV3;
using HVO.WebSite.RoofControlV3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HVO.WebSite.RoofControlV3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        //private readonly IRoofController _roofController;

        public HomeController(ILogger<HomeController> logger/*, IRoofController roofController*/)
        {
            _logger = logger;
            //_roofController = roofController;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}