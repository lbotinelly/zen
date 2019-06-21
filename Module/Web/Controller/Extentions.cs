using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;

namespace Zen.Module.Web.Controller {
    public static class Extentions
    {
        internal static void AddHeaders(this IHeaderDictionary header, Dictionary<string, object> payload)
        {
            if (payload == null) return;

            foreach (var headerItem in payload)
            {
                if (header.ContainsKey(headerItem.Key)) header.Remove(headerItem.Key);
                header.Add(headerItem.Key, headerItem.Value.ToJson());
            }
        }

        internal static void AddModelHeaders<T>(this IHeaderDictionary header) where T : Data<T>
        {
            if (Data<T>.Info<T>.Settings?.Pipelines?.Before != null)
                foreach (var pipelineMember in Data<T>.Info<T>.Settings.Pipelines.Before)
                    AddHeaders(header, pipelineMember.Headers<T>());

            if (Data<T>.Info<T>.Settings?.Pipelines?.After != null)
                foreach (var pipelineMember in Data<T>.Info<T>.Settings.Pipelines.After)
                    AddHeaders(header, pipelineMember.Headers<T>());
        }

        internal static Mutator ToMutator(this IQueryCollection source)
        {
            var modifier = new Mutator { Transform = new QueryTransform() };

            if (source.ContainsKey("sort")) modifier.Transform.OrderBy = source["sort"];

            if (source.ContainsKey("page") || source.ContainsKey("limit"))
            {
                modifier.Transform.PageIndex = source.ContainsKey("page") ? Convert.ToInt32(source["page"]) : 0;
                modifier.Transform.PageSize = source.ContainsKey("limit") ? Convert.ToInt32(source["limit"]) : 50;
            }

            if (source.ContainsKey("filter")) modifier.Transform.Filter = source["filter"];

            if (source.ContainsKey("q")) modifier.Transform.OmniQuery = source["q"];

            if (source.ContainsKey("set")) modifier.SetCode = source["set"];

            return modifier;
        }
    }
}