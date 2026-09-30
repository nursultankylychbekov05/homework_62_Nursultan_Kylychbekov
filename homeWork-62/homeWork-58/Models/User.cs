using Microsoft.AspNetCore.Identity;

namespace InstagramApp.Models;

public class User : IdentityUser
{
    public string Name { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string? Bio { get; set; }
    public string? Gender { get; set; }

    public int PostsCount { get; set; } = 0;
    public int SubscribersCount { get; set; } = 0;
    public int SubscriptionsCount { get; set; } = 0;

    public List<Post> Posts { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
    public List<Like> Likes { get; set; } = new();
}