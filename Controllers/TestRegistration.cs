using Microsoft.AspNetCore.Mvc;

namespace Project3.Controllers;

public class TestRegistration : Controller
{
    public IActionResult Index()
    {
        return View("Home/TestRegistration");
    }
}