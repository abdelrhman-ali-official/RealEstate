namespace Shared.ChatModels
{
    public class ChatRoomDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string User1Id { get; set; }
        public string User2Id { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }
}
