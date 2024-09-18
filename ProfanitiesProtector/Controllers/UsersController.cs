using Microsoft.AspNetCore.Mvc;

using Azure.Storage.Blobs;

using System.Text.Json;

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

        private static List<User> Users = new List<User>
        {
            new User("john.doe@example.com", new Dictionary<string, string> { { "Jane", "jane.smith@example.com" } }),
            new User("jane.smith@example.com", new Dictionary<string, string> { { "John", "john.doe@example.com" } })
        };

        [HttpGet]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            return Ok(Users);
        }

        [HttpGet("{email}")]
        public ActionResult<User> GetUser(string email)
        {
            var user = Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
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
            var user = Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return NotFound();
            }

            user.Contacts = updatedUser.Contacts;
            return NoContent();
        }

        [HttpDelete("{email}")]
        public ActionResult DeleteUser(string email)
        {
            var user = Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return NotFound();
            }

            Users.Remove(user);
            return NoContent();
        }
    }
}