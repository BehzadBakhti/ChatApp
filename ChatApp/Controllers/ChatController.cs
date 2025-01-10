using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers;

public class ChatController : Controller
{
    public IActionResult Index()
    {
        var port = HttpContext.Connection.LocalPort;
        ViewData["WebSocketUrl"] = $"ws://localhost:{port}/ws";
        return View();
    }
}