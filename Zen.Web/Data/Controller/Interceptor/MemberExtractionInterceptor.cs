using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Zen.Base.Extension;
using Zen.Web.Data.Controller.Contracts;

namespace Zen.Web.Data.Controller.Interceptor
{
    public class MemberExtractionInterceptor : IDataControllerPostFetchInterceptor
    {
        public List<JObject> HandleCollection(List<JObject> collection, HttpRequest request)
        {
            var memberDefinition = request.Query["memberExtract"].ToString();

            if (memberDefinition == "") return null;

            var maskDictionary = memberDefinition.FromJson<Dictionary<string, string>>();

            var collectionBuffer = new List<JObject>();

            foreach (var item in collection)
            {
                collectionBuffer.Add(ExtractMembers(item, maskDictionary));
            }

            return collectionBuffer;
        }

        private JObject ExtractMembers(JObject item, Dictionary<string, string> maskDictionary)
        {
            var modelBuffer = new JObject();

            foreach (var member  in maskDictionary)
            {
                var value = item.SelectToken(member.Key)?.ToObject<object>();

                modelBuffer.Add(member.Value, item.SelectToken(member.Key));
            }

            return modelBuffer;
        }
    }
}
