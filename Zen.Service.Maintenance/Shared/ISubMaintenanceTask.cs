using System.Threading.Tasks;
using Zen.Service.Maintenance.Model;

namespace Zen.Service.Maintenance.Shared
{
    public interface ISubMaintenanceTask
    {
        Task<Result> MaintenanceTask();
    }
}