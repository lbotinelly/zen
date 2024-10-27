namespace Zen.Web.OpSec {
    /// <summary>
    ///     Response type indicating the characteristics of a given IP address.
    /// </summary>
    public class IpType
    {
        public bool IsExternal;
        public bool IsInternal;
        public bool IsLocal;
        public bool IsVpn;
        public bool Resolved = true;
    }
}