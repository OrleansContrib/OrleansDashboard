using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    public class BasicAuthenticationOptions : IOptions<BasicAuthenticationOptions>
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public BasicAuthenticationOptions Value => this;
    }
}
