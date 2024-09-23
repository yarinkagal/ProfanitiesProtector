using Microsoft.AspNetCore.Mvc;

using Azure.Storage.Blobs;

using System.Text.Json;
using System.Net;

namespace ProfanitiesProtector
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private string connectionString = Environment.GetEnvironmentVariable("connectionString");
        private string containerName = "users";
        private BlobServiceClient _blobServiceClient;


        public UsersController()
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

    

        [HttpGet("{email}")]
        public async Task<ActionResult<User> > GetUser(string email)
        {
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
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            {
            }

            return Ok();
        }

        [HttpPost]
        [Route("add")]
        public async Task<ActionResult> CreateUser([FromBody] User user)
        {
            try
            {
                var userJson = JsonSerializer.Serialize(user);

                // Create a memory stream for the JSON data
                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(userJson)))
                {
                    var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                    await blobContainerClient.CreateIfNotExistsAsync();

                    var blobClient = blobContainerClient.GetBlobClient(blobName: user.Email);
                    await blobClient.UploadAsync(stream, overwrite: true);


                }
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    

    [HttpPut("{email}")]
        public ActionResult UpdateUser(string email, User updatedUser)
        {
            
            return NoContent();
        }

        [HttpDelete("{email}")]
        public ActionResult DeleteUser(string email)
        {
            
            return NoContent();
        }
    }
}