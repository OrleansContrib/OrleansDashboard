using System.Threading.Tasks;
using OrleansDashboard.Client.Model;

namespace OrleansDashboard
{
    public interface ISiloDetailsProvider
    {
        Task<SiloDetails[]> GetSiloDetails();
    }
}