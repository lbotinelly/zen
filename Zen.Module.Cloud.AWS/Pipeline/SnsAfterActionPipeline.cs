using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Primitives;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Pipeline;

namespace Zen.Module.Cloud.AWS.Pipeline
{
    public abstract class SnsAfterActionPipeline : IAfterActionPipeline
    {
        #region Implementation of IPipelinePrimitive

        public virtual RegionEndpoint Region { get; set; } = RegionEndpoint.USEast1;
        public virtual string Topic { get; set; } = "";

        public string PipelineName => "SNS Announcer";
        public Dictionary<string, object> Headers<T>(ref DataAccessControl accessControl, Dictionary<string, StringValues> requestHeaders, EActionScope scope, T model) where T : Data<T> { return null; }

        public virtual void Process<T>(EActionType type, EActionScope scope, Mutator mutator, T current, T source) where T : Data<T>
        {
            var client = new AmazonSimpleNotificationServiceClient(Region);

            var payload = RenderPayload(type, scope, mutator, current, source);

            SendMessage(client, payload).Wait();
        }

        public virtual PublishRequest RenderPayload<T>(EActionType type, EActionScope scope, Mutator mutator, T current, T source) where T : Data<T>
        {
            return new PublishRequest
            {
                Message =
                    new {type = type.ToString(), scope = scope.ToString(), payload = current ?? source}.ToJson(),
                Subject = $"{(current ?? source).GetFullIdentifier()}",
                TopicArn = Topic
            };
        }

        private static async Task SendMessage(IAmazonSimpleNotificationService snsClient, PublishRequest payload) { await snsClient.PublishAsync(payload); }

        #endregion
    }
}