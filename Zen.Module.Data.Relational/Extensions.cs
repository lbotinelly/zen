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