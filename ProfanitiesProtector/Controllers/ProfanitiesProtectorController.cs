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

        public EmailService EmailService;
        public ProfanitiesProtectorController()
        {
            EmailService = new EmailService();
        }

        [HttpPost]
        [Route("analyze")]
        public async Task AnalyzedChat([FromBody] UnanalyzedChat chat)
        {
            if (chat == null)
            {
                throw new ArgumentNullException(nameof(chat));
            }

            var DetectedChats = new List<AnalyzedChat>();
            foreach (var content in chat.Content) {
                var analyzedChat = new AnalyzedChat(content.ChatName);
                analyzedChat.DetectedMessagesIn = await analyzedChat.AnalyzeMessagesAsync(content.MessagesIn);
                analyzedChat.DetectedMessagesOut = await analyzedChat.AnalyzeMessagesAsync(content.MessagesOut);
                analyzedChat.DetectedImagesIn = await analyzedChat.AnalyzeImagesAsync(content.ImagesIn);
                analyzedChat.DetectedImagesOut = await analyzedChat.AnalyzeImagesAsync(content.ImagesOut);

                if (analyzedChat.HasProfanities())
                {
                    DetectedChats.Add(analyzedChat);

                }
            }

            DetectedChats.ForEach(detectedChat => EmailService.SendEmail(chat.Email, detectedChat));

            AddChatsToUserChats(chat.Email, DetectedChats);

        }

        public async Task AddChatsToUserChats(string email, List<AnalyzedChat> DetectedChats)
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
                analyzedChats.Concat(DetectedChats);
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            {
                analyzedChats = DetectedChats.ToHashSet();
            }

            var analyzedChatsJson = JsonSerializer.Serialize(analyzedChats);
            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(analyzedChatsJson)))
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }
        }
    }
}