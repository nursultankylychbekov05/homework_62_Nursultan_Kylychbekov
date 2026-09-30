namespace InstagramApp.Models;

public class Post
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int LikesCount { get; set; } = 0;
    public int CommentsCount { get; set; } = 0;

    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }

    public List<Comment> Comments { get; set; } = new();
    public List<Like> Likes { get; set; } = new();
}