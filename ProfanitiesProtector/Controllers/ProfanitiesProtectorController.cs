using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace ProfanitiesProtector
{
    
    [ApiController]
    [Route("[controller]")]
    public class ProfanitiesProtectorController : ControllerBase
    {

        private readonly ILogger<ProfanitiesProtectorController> _logger;

        public ProfanitiesProtectorController(ILogger<ProfanitiesProtectorController> logger)
        {
            _logger = logger;
        }



        [HttpGet]
        [Route("")]
        public async Task<AnalyzedChat> AnalyzedChatGet()
        {
            var textToModerate = "Is בת זונה a grabage email bitch abcdef@abcd.com, phone: 4255550111, IP: 255.255.255.255, 1234 Main Boulevard, Panapolis WA 96555.\r\n<offensive word> is the profanity here. Is this information PII? phone 4255550111";


            List<Message> messages = new List<Message>();
            messages.Add(new Message()
            {
                Content = "בן זונה",
                SentByTheChild = true
            });

            messages.Add(new Message()
            {
                Content = "אוהב אותך",
                SentByTheChild = false
            });

            messages.Add(new Message()
            {
                Content = "hello",
                SentByTheChild = true
            });

            UnanalyzedChat chat = new UnanalyzedChat(name: "name1","yarink3@gmail.com", messages);


            if (chat == null)
            {
                throw new ArgumentNullException(nameof(chat));
            }


            var analyzedChat = new AnalyzedChat(chat.ChatName);
            await analyzedChat.AnalyzeChatAsync(chat);
            await AddChatToUserChats(chat.Email, analyzedChat);


            return await Task.FromResult(analyzedChat);

        }

        [HttpPost]
        [Route("analyze")]
        public async Task AnalyzedChat([FromBody] UnanalyzedChat chat)
        {
            if (chat == null)
            {
                throw new ArgumentNullException(nameof(chat));
            }


            var analyzedChat = new AnalyzedChat(chat.ChatName);
            await analyzedChat.AnalyzeChatAsync(chat);
            if (analyzedChat.HasProfanities())
            {
                AddChatToUserChats(chat.Email, analyzedChat);
            }

        }

        public async Task AddChatToUserChats(string email, AnalyzedChat newChat)
        {
            string connectionString = Environment.GetEnvironmentVariable("connectionString");


            var containerName = "analyzedchats";
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);



            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync();

            var blobClient = blobContainerClient.GetBlobClient(blobName: email);

            HashSet<AnalyzedChat> analyzedChats;
            try
            {
                var analyzedChatsStreamBlob = await blobClient.DownloadAsync();
                var analyzedChatsStream = analyzedChatsStreamBlob.Value.Content;

                using (var reader = new StreamReader(analyzedChatsStream))
                {
                    var content = await reader.ReadToEndAsync();
                    analyzedChats = JsonSerializer.Deserialize<HashSet<AnalyzedChat>>(content);
                }
                analyzedChats.Add(newChat);
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            {
                analyzedChats = new HashSet<AnalyzedChat> { newChat };
            }

            var analyzedChatsJson = JsonSerializer.Serialize(analyzedChats);
            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(analyzedChatsJson)))
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }
        }
    }
}