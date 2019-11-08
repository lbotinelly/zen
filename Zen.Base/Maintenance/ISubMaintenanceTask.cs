using System.Threading.Tasks;

namespace Zen.Base.Maintenance {
    public interface ISubMaintenanceTask
    {
        Task<Result> MaintenanceTask();
    }
}