using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InstagramApp.Models;

namespace InstagramApp.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public UsersController(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    
    [HttpGet]
    public async Task<IActionResult> Search(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return View(new List<User>());
        }

        string q = query.ToLower();
        var users = await _context.Users
            .Where(u => u.UserName!.ToLower().Contains(q) ||
                        u.Email!.ToLower().Contains(q) ||
                        (u.Name != null && u.Name.ToLower().Contains(q)) ||
                        (u.Bio != null && u.Bio.ToLower().Contains(q)))
            .ToListAsync();

        ViewBag.Query = query;
        return View(users);
    }
    
    [HttpGet]
    public async Task<IActionResult> Profile(string id)
    {
        var user = await _context.Users
            .Include(u => u.Posts.OrderByDescending(p => p.CreatedAt))
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound();

        var currentUserId = _userManager.GetUserId(User);
        bool isFollowing = await _context.Subscriptions
            .AnyAsync(s => s.FollowerId == currentUserId && s.TargetUserId == id);

        ViewBag.IsFollowing = isFollowing;
        ViewBag.IsCurrentUser = currentUserId == id;

        return View(user);
    }
    
    [HttpPost]
    public async Task<IActionResult> ToggleFollow(string targetUserId)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (currentUserId == targetUserId) return BadRequest();

        var follower = await _context.Users.FindAsync(currentUserId);
        var targetUser = await _context.Users.FindAsync(targetUserId);

        if (follower == null || targetUser == null) return NotFound();

        var existingSub = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.FollowerId == currentUserId && s.TargetUserId == targetUserId);

        bool isFollowing;

        if (existingSub != null)
        {
            _context.Subscriptions.Remove(existingSub);
            follower.SubscriptionsCount = Math.Max(0, follower.SubscriptionsCount - 1);
            targetUser.SubscribersCount = Math.Max(0, targetUser.SubscribersCount - 1);
            isFollowing = false;
        }
        else
        {
            _context.Subscriptions.Add(new Subscription
            {
                FollowerId = currentUserId!,
                TargetUserId = targetUserId
            });
            follower.SubscriptionsCount++;
            targetUser.SubscribersCount++;
            isFollowing = true;
        }

        await _context.SaveChangesAsync();
        
        return Json(new { success = true, isFollowing = isFollowing, subscribersCount = targetUser.SubscribersCount });
    }
}