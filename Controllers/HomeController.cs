using Microsoft.AspNetCore.Mvc;

namespace dotNetCore_websokect_demo.Controllers
{
    public class HomeController:Controller
    {
        public IActionResult Index() => View("index.html");
    }
}