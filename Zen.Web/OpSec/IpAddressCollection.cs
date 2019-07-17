using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace Zen.Web.OpSec {
    public class IpAddressCollection : IEnumerable<IPAddress>, IEnumerator<IPAddress>
    {
        private readonly Network.IpNetwork _ipnetwork;
        private double _enumerator;

        internal IpAddressCollection(Network.IpNetwork ipnetwork)
        {
            _ipnetwork = ipnetwork;
            _enumerator = -1;
        }

        #region Count, Array, Enumerator

        public double Count => _ipnetwork.Usable + 2;

        public IPAddress this[double i]
        {
            get
            {
                if (i >= Count) throw new ArgumentOutOfRangeException("i");

                var ipn = Network.IpNetwork.Subnet(_ipnetwork, 32);
                return ipn[i].Network;
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator<IPAddress> IEnumerable<IPAddress>.GetEnumerator() { return this; }

        IEnumerator IEnumerable.GetEnumerator() { return this; }

        #region IEnumerator<IPNetwork> Members

        public IPAddress Current { get { return this[_enumerator]; } }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            // nothing to dispose
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current { get { return Current; } }

        public bool MoveNext()
        {
            _enumerator++;
            if (_enumerator >= Count) return false;
            return true;
        }

        public void Reset() { _enumerator = -1; }

        #endregion

        #endregion
    }
}