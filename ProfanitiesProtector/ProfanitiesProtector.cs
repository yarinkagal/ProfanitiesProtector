
using System.ComponentModel.DataAnnotations;

namespace ProfanitiesProtector
{
    
    public class ProfanitiesProtector
    {

        public ProfanitiesProtector()
        {
        }

    }

    public class Message
    {
        public required string Content { get; set; }
        public bool SentByTheChild { get; set; }
        public string? ImageUrl { get; set; }
    }

    

    public class UnanalyzedChat
    {
        public string ChatName { get; set; }
        public List<Message> Messages { get; set; }
        public string Email { get; set; }

        // Parameterless constructor for deserialization
        public UnanalyzedChat() { }

        public UnanalyzedChat(string name,string email, List<Message> messages)
        {
            ChatName = name;
            Messages = new List<Message>(messages);
            Email = email;
        }
    }


}