using System;
using Microsoft.Extensions.Logging;

namespace Zen.Web.Internal.Infrastructure
{
    internal interface IZenTrace : ILogger
    {
        void ConnectionStart(string connectionId);

        void ConnectionStop(string connectionId);

        void ConnectionPause(string connectionId);

        void ConnectionResume(string connectionId);

        void ConnectionRejected(string connectionId);

        void ConnectionKeepAlive(string connectionId);

        void ConnectionDisconnect(string connectionId);

        void RequestProcessingError(string connectionId, Exception ex);

        void ConnectionHeadResponseBodyWrite(string connectionId, long count);

        void NotAllConnectionsClosedGracefully();

        void ApplicationError(string connectionId, string traceIdentifier, Exception ex);

        void NotAllConnectionsAborted();

        void HeartbeatSlow(TimeSpan interval, DateTimeOffset now);

        void ApplicationNeverCompleted(string connectionId);

        void RequestBodyStart(string connectionId, string traceIdentifier);

        void RequestBodyDone(string connectionId, string traceIdentifier);

        void RequestBodyNotEntirelyRead(string connectionId, string traceIdentifier);

        void RequestBodyDrainTimedOut(string connectionId, string traceIdentifier);

        void RequestBodyMinimumDataRateNotSatisfied(string connectionId, string traceIdentifier, double rate);

        void ResponseMinimumDataRateNotSatisfied(string connectionId, string traceIdentifier);

        void ApplicationAbortedConnection(string connectionId, string traceIdentifier);

        void Http2ConnectionClosing(string connectionId);

        void Http2ConnectionClosed(string connectionId, int highestOpenedStreamId);
    }
}