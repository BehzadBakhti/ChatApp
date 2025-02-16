namespace ChatApp.Models;

public class ChatMessageEntity
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public UserEntity Sender { get; set; }
    
    public int ReceiverId { get; set; }
    public UserEntity Receiver { get; set; }
    public required string? MessageBody { get; set; }
    public DateTime TimeStamp { get; set; }

}