namespace ProfanitiesProtector
{
    public class User
    {
        public string Email { get; set; }
        public Dictionary<string, string> Contacts { get; set; }

        public User(string email, Dictionary<string, string> contacts)
        {
            Email = email;
            Contacts = contacts;
        }

        public User()
        {
        }   
    }
}