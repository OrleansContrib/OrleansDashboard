namespace OrleansDashboard
{
    internal class UserCredentials
    {
        public UserCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; }

        public string Password { get; }

        public bool HasValue()
        {
            return false == string.IsNullOrEmpty(Username)
                   && false == string.IsNullOrEmpty(Password);
        }
    }
}