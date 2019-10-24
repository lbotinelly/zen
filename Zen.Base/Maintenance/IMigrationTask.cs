using System.Threading.Tasks;

namespace Zen.Base.Maintenance {
    public interface IMigrationTask
    {
        Task<Result> MigrationTask();
    }
}