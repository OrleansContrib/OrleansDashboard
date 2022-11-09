namespace OrleansDashboard
{
    public sealed class DashboardOptions
    {
        /// <summary>
        ///   The URL path the dashboard web server will listen on.
        ///   The default is '/'.
        /// </summary>
        public string BasePath { get; set; } = "/";

        /// <summary>
        ///   The URL path the dashboard will attempt to load the javascript file from.
        ///   The default is null, which will use the value for BasePath.
        /// </summary>
        public string ScriptPath { get; set; }

        /// <summary>
        ///   The URL path the dashboard will attempt to load a custom CSS file from on top of the default CSS file
        ///   The default is null, which will not load in a custom CSS file
        /// </summary>
        public string CustomCssPath { get; set; } 

        /// <summary>
        ///   Username for basic auth
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///   Password for basic auth
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///   The host name for the web sever to bind to
        /// </summary>
        public string Host { get; set; } = "*";

        /// <summary>
        ///   Disables the trace feature.
        /// </summary>
        public bool HideTrace { get; set; }

        /// <summary>
        ///   Set to 'false' to disable the dashboard from self hosting, and host it yourself as a middleware component.
        /// </summary>
        public bool HostSelf { get; set; } = true;

        /// <summary>
        ///   Number of ms between counter samples.
        /// </summary>
        public int CounterUpdateIntervalMs { get; set; } = 1000;

        /// <summary>
        ///   The length of the history.
        /// </summary>
        public int HistoryLength { get; set; } = 100;

        /// <summary>
        ///   The port number the dashboard will use for self-hosting.
        /// </summary>
        public int Port { get; set; } = 8080;

        internal bool HasUsernameAndPassword()
        {
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        }
    }
}
