using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InstagramApp.Models;

namespace InstagramApp.Controllers;

[Authorize]
public class PostsController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _env;

    public PostsController(
        AppDbContext context, 
        UserManager<User> userManager, 
        IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
    }

    [HttpGet]
    public IActionResult Create() => View();
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IFormFile imageFile, string? description)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            ModelState.AddModelError("", "Загрузите изображение для публикации");
            return View();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return RedirectToAction("Login", "Account");

        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/posts");
        Directory.CreateDirectory(uploadsFolder);
        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(fileStream);
        }

        var post = new Post
        {
            ImageUrl = "/uploads/posts/" + uniqueFileName,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            LikesCount = 0,        
            CommentsCount = 0,    
            UserId = currentUser.Id
        };

        _context.Posts.Add(post);

        currentUser.PostsCount++;
        _context.Users.Update(currentUser);

        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Home");
    }
    
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var post = await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments.OrderBy(c => c.CreatedAt))
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return NotFound();

        var currentUserId = _userManager.GetUserId(User);
        ViewBag.HasLiked = post.Likes.Any(l => l.UserId == currentUserId);

        return View(post);
    }
    
    [HttpPost]
    public async Task<IActionResult> ToggleLike(int postId)
    {
        var currentUserId = _userManager.GetUserId(User);
        var post = await _context.Posts.FindAsync(postId);

        if (post == null) return NotFound();

        var existingLike = await _context.Likes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == currentUserId);

        bool isLiked;

        if (existingLike != null)
        {
            _context.Likes.Remove(existingLike);
            post.LikesCount = Math.Max(0, post.LikesCount - 1);
            isLiked = false;
        }
        else
        {
            _context.Likes.Add(new Like { PostId = postId, UserId = currentUserId! });
            post.LikesCount++;
            isLiked = true;
        }

        await _context.SaveChangesAsync();

        return Json(new { success = true, isLiked = isLiked, likesCount = post.LikesCount });
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int postId, string text, string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(text))
            return RedirectToAction("Details", new { id = postId });

        var currentUserId = _userManager.GetUserId(User);
        var post = await _context.Posts.FindAsync(postId);

        if (post == null) return NotFound();

        var comment = new Comment
        {
            PostId = postId,
            UserId = currentUserId!,
            Text = text,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        post.CommentsCount++;

        await _context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Details", new { id = postId });
    }
    
    [HttpPost]
    public async Task<IActionResult> DeletePost(int id)
    {
        var currentUserId = _userManager.GetUserId(User);
        var post = await _context.Posts.FindAsync(id);

        if (post == null || post.UserId != currentUserId) return NotFound();

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> EditPostDescription(int id, string description)
    {
        var currentUserId = _userManager.GetUserId(User);
        var post = await _context.Posts.FindAsync(id);

        if (post == null || post.UserId != currentUserId) return NotFound();

        post.Description = description;
        await _context.SaveChangesAsync();

        return Json(new { success = true, description = post.Description });
    }
}