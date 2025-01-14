using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace ChatApp.Controllers;

public class ChatController : Controller
{
    private readonly IUserService _userService;
    public ChatController(IUserService userService)
    {
        _userService = userService;
    }
    public IActionResult Index(string username, string avatar)
    {
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Index", "Home");
        }
        var newUserId = _userService.AddUser(username, avatar);
        var activeUsers = _userService.GetActiveUsers();
        ViewData["Username"] = username;
        ViewData["Avatar"] = avatar;
        ViewData["UserId"] = newUserId;
        ViewData["ActiveUsers"] = activeUsers;
        return View();
    }
}