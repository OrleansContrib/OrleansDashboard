using System.Threading.Tasks;
using OrleansDashboard.Client.Model;

namespace OrleansDashboard.Metrics.Details
{
    public interface ISiloDetailsProvider
    {
        Task<SiloDetails[]> GetSiloDetails();
    }
}