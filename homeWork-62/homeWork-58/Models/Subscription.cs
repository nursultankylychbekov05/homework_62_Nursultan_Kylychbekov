namespace InstagramApp.Models;

public class Subscription
{
    public int Id { get; set; }
    public string FollowerId { get; set; } = string.Empty; 
    public User? Follower { get; set; }

    public string TargetUserId { get; set; } = string.Empty; 
    public User? TargetUser { get; set; }
}