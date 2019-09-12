using System.Data.Common;
using Oracle.ManagedDataAccess.Client;
using Zen.Base.Module;
using Zen.Module.Data.Relational;

namespace Zen.Module.Data.Oracle
{
    public class OracleAdapter : RelationalAdapter
    {
        #region Overrides of RelationalAdapter

        public override DbConnection GetConnection<T>() { return new OracleConnection(Data<T>.Info<T>.Settings.ConnectionString); }

        #endregion
    }
}