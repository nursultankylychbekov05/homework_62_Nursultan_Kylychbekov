using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InstagramApp.Models;

namespace InstagramApp.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public HomeController(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    
    public async Task<IActionResult> Index()
    {
        var currentUserId = _userManager.GetUserId(User);
        
        var followedUserIds = await _context.Subscriptions
            .Where(s => s.FollowerId == currentUserId)
            .Select(s => s.TargetUserId)
            .ToListAsync();

        followedUserIds.Add(currentUserId!);

        var posts = await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .ThenInclude(c => c.User)
            .Where(p => followedUserIds.Contains(p.UserId))
            .OrderByDescending(p => p.CreatedAt) 
            .ToListAsync();

        ViewBag.CurrentUserId = currentUserId;
        return View(posts);
    }
}