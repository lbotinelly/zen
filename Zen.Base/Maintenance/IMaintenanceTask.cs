using System.Threading.Tasks;

namespace Zen.Base.Maintenance
{
    public interface IMaintenanceTask
    {
        Task<Result> MaintenanceTask();
    }
}