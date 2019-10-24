using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Zen.Base.Module.Log
{
    public class Clicker
    {
        private static readonly NumberFormatInfo Format = new NumberFormatInfo {PercentPositivePattern = 1, PercentNegativePattern = 1};
        private string _message;
        private long _pIndex;
        private int _pNotifySlice;
        private int _pNotifySliceMoreInfo;

        private Stopwatch _s;

        public Clicker(string message, IEnumerable<object> source) { Start(message, source.Count()); }

        public Clicker(string message, long count) { Start(message, count); }
        public Clicker(string message, long count, int sliceSize) { Start(message, count, sliceSize); }

        public long Count { get; private set; }

        public void Start(string message, long count, int pNotifySlice = -1)
        {
            _message = message;
            Count = count;

            if (pNotifySlice == -1) // auto
            {
                var digits = count.ToString().Length;
                pNotifySlice = digits < 3 ? 100 : Convert.ToInt32("1" + new string('0', digits - 2));
            }

            _pNotifySlice = pNotifySlice;
            _pNotifySliceMoreInfo = 0;

            _s = new Stopwatch();
            _s.Start();

            Current.Log.Add($"{_message}: START ({count}) items", Message.EContentType.Info);
        }

        public void Click()
        {
            _pIndex++;
            if( (_pIndex % _pNotifySlice != 0 ) && _pIndex != 0)return;

            _pNotifySliceMoreInfo++;
            _pNotifySliceMoreInfo = _pNotifySliceMoreInfo % 10;

            var part = (double) _pIndex / Count;
            var partStr = part.ToString("P2", Format).PadLeft(7);

            var invPart = TimeSpan.FromMilliseconds(_s.ElapsedMilliseconds * (1 / part));

            var charSlots = Count.ToString().Length;

            var sIndex = _pIndex.ToString().PadLeft(charSlots);

            var currT = _s.Elapsed.ToString(@"\:hh\:mm\:ss");
            var leftT = invPart.Subtract(_s.Elapsed).ToString(@"\:hh\:mm\:ss");
            var totlT = invPart.ToString(@"\:hh\:mm\:ss");

            var msg = $"{_message}:        {sIndex}/{Count} {partStr} E{currT} L{leftT} T{totlT}";

            Current.Log.Add(msg, _pNotifySliceMoreInfo == 0 ? Message.EContentType.MoreInfo : Message.EContentType.Debug);
        }

        public void End()
        {
            _s.Stop();
            var regPerSec = Count / ((double) _s.ElapsedMilliseconds / 1000);
            Current.Log.Add($"{_message}: END   ({_s.Elapsed} elapsed, {regPerSec:F2} items/sec)", Message.EContentType.Info);
        }
    }
}