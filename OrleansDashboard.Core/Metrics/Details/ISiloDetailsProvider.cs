using System.Threading.Tasks;
using OrleansDashboard.Model;

namespace OrleansDashboard.Metrics.Details
{
    public interface ISiloDetailsProvider
    {
        Task<SiloDetails[]> GetSiloDetails();
    }
}