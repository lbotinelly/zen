using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Zen.Base;

namespace Zen.Web.Model.State {
    public class ZenDistributedSessionStore : ISessionStore
    {
        #region Implementation of ISessionStore

        public ISession Create(string sessionKey, TimeSpan idleTimeout, TimeSpan ioTimeout, Func<bool> tryEstablishSession, bool isNewSessionKey)
        {
            if (string.IsNullOrEmpty(sessionKey)) throw new ArgumentException(ConstantStrings.ArgumentCannotBeNullOrEmpty, nameof(sessionKey));

            if (tryEstablishSession == null) throw new ArgumentNullException(nameof(tryEstablishSession));

            return new ZenDistributedSession(sessionKey, idleTimeout, ioTimeout, tryEstablishSession, isNewSessionKey);
        }

        #endregion
    }
}