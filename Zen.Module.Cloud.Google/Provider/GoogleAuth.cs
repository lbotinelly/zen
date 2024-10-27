using Zen.Base.Common;
using Zen.Web.Auth.Provider;

namespace Zen.Module.Cloud.Google.Provider
{
    [Priority(Level = -2)]
    public abstract class GoogleAuth : IAuthPrimitive
    {
        public string SenderId { get; set; }
        public string ServerKey { get; set; }
        public string Code { get; } = "Google";

        #region Implementation of IZenProvider

        public EOperationalStatus OperationalStatus { get; set; } = EOperationalStatus.Undefined;

        public virtual void Initialize()
        {
            OperationalStatus = EOperationalStatus.Operational;
        }
        public virtual string GetState() => $"{OperationalStatus}";
        #endregion
    }
}