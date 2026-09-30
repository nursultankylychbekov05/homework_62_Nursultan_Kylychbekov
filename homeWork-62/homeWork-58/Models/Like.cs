namespace InstagramApp.Models;

public class Like
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }

    public int PostId { get; set; }
    public Post? Post { get; set; }
}