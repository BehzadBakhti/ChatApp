namespace ChatApp;

public class ChatMessage
{
    public required string SenderId { get; set; }
    public required string ReceiverId { get; set; }
    public required string? MessageBody { get; set; }
    //public DateTime TimeStamp { get; set; }
}