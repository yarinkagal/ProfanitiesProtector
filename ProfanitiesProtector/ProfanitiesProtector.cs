
using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;

namespace ProfanitiesProtector
{
    
    public class ProfanitiesProtector
    {

        public ProfanitiesProtector()
        {
        }

    }

    public class UnanalyzedChatContent
    {
        public string ChatName { get; set; }
        public List<string> MessagesIn { get; set; }
        public List<string> MessagesOut { get; set; }
        public List<string> ImagesIn { get; set; }
        public List<string> ImagesOut { get; set; }

        public UnanalyzedChatContent(string ChatName, List<string> MessagesIn, List<string> MessagesOut, List<string> ImagesIn, List<string> ImagesOut  )
        {
            ChatName = ChatName;
            MessagesIn = new List<string>(MessagesIn);
            MessagesOut = new List<string>(MessagesOut);
            ImagesIn = ImagesIn;
            ImagesOut = ImagesOut;
        }

        public UnanalyzedChatContent() { }
    }
    public class UnanalyzedChat
    {
        
        public List<UnanalyzedChatContent> Content { get; set; }
        public string Email { get; set; }

        // Parameterless constructor for deserialization
        public UnanalyzedChat() { }

        public UnanalyzedChat(string Email, List<UnanalyzedChatContent> Content )
        {
            Email = Email;
            Content = Content;


        }
    }


}