using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace OrleansDashboard.Implementation.Assets
{
    public interface IAssetProvider
    {
        Task ServeAssetAsync(string name, HttpContext httpContext);
    }
}
