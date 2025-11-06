namespace Shared.ChatModels
{
    public class ContactPropertyViewerRequest
    {
        public int PropertyId { get; set; }
        public string ViewerUserId { get; set; }
        public string Message { get; set; }
    }
}