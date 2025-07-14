namespace Shared.ChatModels
{
    public class SendMessageRequest
    {
        public string Message { get; set; }
        public int? RepliedToMessageId { get; set; }
    }
} 