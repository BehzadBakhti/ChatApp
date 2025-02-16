using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace ChatApp.Controllers;

public class ChatController : Controller
{
    private readonly IUserService _userService;
    private readonly IChatService _chatService;
    public ChatController(IUserService userService, IChatService chatService)
    {
        _userService = userService;
        _chatService = chatService;
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

    [HttpGet]
    public IActionResult GetUserChats(string myUserId, string otherUserId)
    {
        // Simulate fetching chat messages from memory (or a database in the future)
        var chats = _chatService.GetChatsForUser(myUserId, otherUserId);
        return Json(chats);
    }
}