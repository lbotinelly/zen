using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Zen.Base;

namespace Zen.Web.Model.State
{
    public class ZenDistributedSession : ISession
    {
        private readonly TimeSpan _idleTimeout;
        private readonly TimeSpan _ioTimeout;
        private readonly bool _isNewSessionKey;
        private readonly Func<bool> _tryEstablishSession;
        private bool _isAvailable;
        private bool _isModified;

        private IDictionary<string, byte[]> _store;

        private ZenSession _sourceModel;

        public ZenDistributedSession(
            string sessionKey,
            TimeSpan idleTimeout,
            TimeSpan ioTimeout,
            Func<bool> tryEstablishSession,
            bool isNewSessionKey)
        {
            if (string.IsNullOrEmpty(sessionKey)) throw new ArgumentException(ConstantStrings.ArgumentCannotBeNullOrEmpty, nameof(sessionKey));

            Id = sessionKey;
            _idleTimeout = idleTimeout;
            _ioTimeout = ioTimeout;
            _tryEstablishSession = tryEstablishSession ?? throw new ArgumentNullException(nameof(tryEstablishSession));
            _store = new Dictionary<string, byte[]>();
            _isNewSessionKey = isNewSessionKey;
        }

        public bool IsAvailable
        {
            get
            {
                Load();
                return _isAvailable;
            }
        }

        public string Id { get; private set; }

        public IEnumerable<string> Keys
        {
            get
            {
                Load();
                return _store.Keys;
            }
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            Load();
            return _store.TryGetValue(key, out value);
        }

        public void Set(string key, byte[] value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (IsAvailable)
            {
                if (!_tryEstablishSession()) throw new InvalidOperationException("Invalid Session Establishment");

                var copy = new byte[value.Length];
                Buffer.BlockCopy(value, 0, copy, 0, value.Length);
                _store[key] = copy;
                _isModified = true;
            }
        }

        public void Remove(string key)
        {
            Load();
            _store.Remove(key);
            _isModified = true;
        }

        public void Clear()
        {
            Load();
            _store.Clear();
            _isModified = true;
        }

        // This will throw if called directly and a failure occurs. The user is expected to handle the failures.
        public async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            if (_sourceModel != null)
            {
                using (var timeout = new CancellationTokenSource(_ioTimeout))
                {
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
                    try
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        await Task.Run(() => { Load(); }, cts.Token);
                    }
                    catch (OperationCanceledException oex)
                    {
                        if (!timeout.Token.IsCancellationRequested) throw;

                        Base.Current.Log.Warn<ZenDistributedSession>($"TimeOut loading session {Id}");
                        throw new OperationCanceledException("Timed out loading the ZenSession.", oex, timeout.Token);
                    }
                }

                _isAvailable = true;
            }
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            using (var timeout = new CancellationTokenSource(_ioTimeout))
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
                if (_isModified)
                    try
                    {
                        cts.Token.ThrowIfCancellationRequested();

                        await Task.Run(() => { Save(); }, cts.Token);
                    }
                    catch (OperationCanceledException oex)
                    {
                        if (timeout.Token.IsCancellationRequested) throw new OperationCanceledException("Timed out committing the ZenSession.", oex, timeout.Token);

                        throw;
                    }
            }
        }

        private void Load()
        {
            if (_sourceModel != null) return;

            try
            {
                _sourceModel = ZenSession.Get(Id) ?? new ZenSession {Id = Id};

                if (_sourceModel != null) FetchFromSourceModel();

                _isAvailable = true;
                _isModified = false;
            }
            catch (Exception exception)
            {
                Base.Current.Log.Warn<ZenDistributedSession>($"Read Exception: {Id}");
                Base.Current.Log.Add<ZenDistributedSession>(exception);

                _isAvailable = false;
                Id = null;
                _store = null;
            }
        }

        private void Save()
        {
            try
            {
                var session = ZenSession.Get(Id) ?? new ZenSession {Id = Id};

                var mustSave = _isModified || session.LastUpdate?.AddSeconds(60) < DateTime.Now;

                if (!mustSave) return;

                session.Store = _store;
                session.LastUpdate = DateTime.Now;
                session.Save();
            }
            catch (Exception exception)
            {
                Base.Current.Log.Warn<ZenDistributedSession>($"Write Exception: {Id}");
                Base.Current.Log.Add<ZenDistributedSession>(exception);
            }
        }

        private void FetchFromSourceModel()
        {
            _store = _sourceModel?.Store ?? new ConcurrentDictionary<string, byte[]>();

            // Want a nice stackOverflowException? Allow the next line to run. (loop on Log generation, because of attempted User resolution)
            // Base.Current.Log.Info<ZenDistributedSession>("Session Loaded"); 
        }
    }
}