using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Pebble.Database.Common;

namespace Zen.Module.Data.Relational
{
    public static class Extensions
    {
        public static Mutator SetStatement(this Mutator mutator, string statement)
        {
            if (statement == null) return mutator;
            if (mutator == null) mutator = new Mutator();

            if (mutator.Transform == null) mutator.Transform = new QueryTransform();
            if (mutator.Transform.Statement == null) mutator.Transform.Statement = statement;

            return mutator;
        }

        public static SqlBuilder.Template ToSqlBuilderTemplate<T>(this Mutator mutator, Settings<T> settings, StatementMasks masks) where T : Data<T> => mutator.ToSqlBuilderTemplate(settings.Members, masks);

        public static SqlBuilder.Template ToSqlBuilderTemplate(this Mutator mutator, Dictionary<string, MemberAttribute> settingsMembers = null, StatementMasks masks = null)
        {
            if (mutator == null) return null;

            var ret = new SqlBuilder();

            var template = mutator.Transform.Statement;

            if (mutator.Transform?.Filter != null)
            {
                if (!template.Contains("where", StringComparison.InvariantCultureIgnoreCase)) template += " /**where**/";

                var parameterPrefix = masks.ParameterPrefix;

                var filter = mutator.Transform?.Filter.FromJson<Dictionary<string, object>>();
                var filterParameters = filter.AddPrefix(parameterPrefix);

                var fieldSet = string.Join(", ", filter.Keys.Select(i => $"{i} = {parameterPrefix}{i}"));

                ret.OrWhere(fieldSet, filterParameters);
            }

            if (!string.IsNullOrEmpty(mutator.Transform.OrderBy))
            {
                if (!template.Contains("/**orderby**/")) template += " /**orderby**/";

                var field = mutator.Transform.OrderBy;
                var direction = "";

                if (field[0] == '-')
                {
                    field = field.Substring(1);
                    direction = " DESC";
                }

                if (field[0] == '+') field = field.Substring(1);

                if (settingsMembers != null)
                    if (settingsMembers.ContainsKey(field))
                        field = settingsMembers[field].TargetName;
                ret.OrderBy(field + direction);
            }

            var selector = ret.AddTemplate(template);
            ret.Select(mutator.Transform?.OutputMembers ?? "*");

            return selector;
        }

        public static ModelDescriptor ToModelDescriptor<T>() where T : Data<T>
        {
            var settings = Info<T>.Settings;

            var res = new ModelDescriptor
            {
                Collection = new ModelDescriptor.CollectionDescriptor
                {
                    SourceName = settings.TypeQualifiedName,
                    TargetName = settings.StorageCollectionName
                },
                Members = settings.Members.ToModelDescriptorMembers()
            };

            return res;
        }

        public static Dictionary<string, ModelDescriptor.MemberDescriptor> ToModelDescriptorMembers(this Dictionary<string, MemberAttribute> source) => source.ToDictionary(i => i.Key, i => i.Value.ToMemberDescriptor());

        public static ModelDescriptor.MemberDescriptor ToMemberDescriptor(this MemberAttribute source)
        {
            var ret = new ModelDescriptor.MemberDescriptor
            {
                Length = source.Size,
                ValueType = source.Type,
                TargetName = source.TargetName,
                SourceName = source.SourceName,
                MemberSourceType = source.Interface == EMemberType.Field ? ModelDescriptor.MemberDescriptor.EMemberSourceType.Field : ModelDescriptor.MemberDescriptor.EMemberSourceType.Property
            };

            return ret;
        }
    }
}