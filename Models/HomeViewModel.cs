namespace NYPTIQ.Models
{
    public class ConversationItem
    {
        public bool IsHuman { get; set; }
        public string Text { get; set; }
    }

    public class HomeViewModel
    {
        public string Prompt { get; set; }
        public string GeneratedText { get; set; }
        public List<ConversationItem> Conversation { get; set; } = new List<ConversationItem>();
    }
}
