using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Pipeline;

namespace Zen.Web.Data.Controller
{
    public static class Extensions
    {
        internal static void AddHeaders(this IHeaderDictionary header, Dictionary<string, object> payload)
        {
            if (payload == null) return;

            foreach (var (key, value) in payload) header.AddHeader(key, value);
        }

        internal static void AddHeader(this IHeaderDictionary header, string key, object value)
        {
            if (value == null) return;
            if (header.ContainsKey(key)) header.Remove(key);
            header.Add(key, value.ToJson());
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

        internal static Mutator ToMutator<T>(this IQueryCollection source) where T : Data<T>
        {
            var modifier = new Mutator { Transform = new QueryTransform() };

            if (Data<T>.Info<T>.Settings?.Pipelines?.Before != null)
            {
                // Transform it only once per request.
                var stringQueryCollection = source.ToDictionary(i => i.Key, i => i.Value.ToList());

                foreach (var pipelineMember in Data<T>.Info<T>.Settings.Pipelines.Before)
                    modifier.AddPipelineMetadata(pipelineMember, stringQueryCollection);
            }

            if (source.ContainsKey("sort")) modifier.Transform.OrderBy = source["sort"];

            if (source.ContainsKey("page") || source.ContainsKey("size"))
                modifier.Transform.Pagination = new Pagination
                {
                    Index = source.ContainsKey("page") ? Convert.ToInt32(source["page"]) : 0,
                    Size = source.ContainsKey("size") ? Convert.ToInt32(source["size"]) : 50
                };

            if (source.ContainsKey("filter")) modifier.Transform.Filter = source["filter"];

            if (source.ContainsKey("q")) modifier.Transform.OmniQuery = source["q"];

            return modifier;
        }

        private static void AddPipelineMetadata(this Mutator mutator, IBeforeActionPipeline pipelineMember, Dictionary<string, List<string>> headerData)
        {
            var postProcessContent = pipelineMember.ParseRequest(headerData);
            if (postProcessContent.HasValue) mutator.PipelineMetadata[postProcessContent.Value.Key] = postProcessContent.Value.Value;
        }

        internal static void AddMutatorHeaders<T>(this IHeaderDictionary header, Mutator mutator) where T : Data<T>
        {
            if (mutator.Transform.Pagination == null) return;

            var count = Data<T>.Count(mutator);
            var pages = count < 2 ? count : (int)((count - 1) / mutator.Transform.Pagination.Size) + 1;

            header.AddHeader("x-zen-pagination",
                new
                {
                    page = mutator.Transform.Pagination.Index,
                    size = mutator.Transform.Pagination.Size,
                    count,
                    pages
                });
        }
    }
}