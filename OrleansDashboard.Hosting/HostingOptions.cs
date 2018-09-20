namespace OrleansDashboard.Hosting
{
    public sealed class HostingOptions
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Host { get; set; } = "*";

        public bool HideTrace { get; set; }

        public bool HostSelf { get; set; } = true;

        public int Port { get; set; } = 8080;

        public string BasePath { get; set; } = "/";

        public bool HasUsernameAndPassword()
        {
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        }
    }
}