using System.Collections.Generic;
using System.Linq;
using Dapper;
using Zen.Base.Extension;
using Zen.Base.Module.Data;

namespace Zen.Module.Data.Relational
{
    public static class Extensions
    {
        public static SqlBuilder.Template ToSqlBuilderTemplate(this Mutator mutator)
        {
            var ret = new SqlBuilder();

            if (mutator == null) return null;

            var selector = ret.AddTemplate(mutator.Transform.Statement);

            ret.Select(mutator.Transform?.OutputMembers ?? "*");

            if (mutator.Transform?.Filter == null) return selector;

            var filterParameters = mutator.Transform?.Filter.FromJson<Dictionary<string, object>>();

            var fieldSet = filterParameters.Keys.Select(i => $"{i} = @{i}");

            ret.OrWhere(string.Join(", ", fieldSet), filterParameters.ToObject());

            return selector;
        }
    }
}