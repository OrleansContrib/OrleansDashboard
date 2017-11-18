﻿namespace OrleansDashboard
{
    public sealed class DashboardOptions
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Host { get; set; } = "*";
    
        public bool HideTrace { get; set; }

        public bool HostSelf { get; set; } = true;

        public int Port { get; set; } = 8080;

        public bool HasUsernameAndPassword()
        {
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        }
    }
}
