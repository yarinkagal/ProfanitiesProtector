namespace ProfanitiesProtector
{
    public class User
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public Dictionary<string, string> Contacts { get; set; }

        public User(string email,string password, Dictionary<string, string> contacts)
        {
            Email = email;
            Password = password;
            Contacts = contacts;
        }

        public User()
        {
        }   
    }
}