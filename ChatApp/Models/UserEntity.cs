namespace ChatApp.Models;

public class UserEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Avatar { get; set; }
    public string? Email { get; set; }

}