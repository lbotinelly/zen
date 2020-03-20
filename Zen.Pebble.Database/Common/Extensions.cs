using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Zen.Pebble.Database.Cache;

namespace Zen.Pebble.Database.Common
{
    public static class Extensions
    {
        public static string Format(this string source, object value) { return string.Format(source, value); }

        public static string Format(this string source, params object[] values) { return string.Format(source, values); }

        public static string Format(this string source, IFormatProvider provider, object[] values) { return string.Format(provider, source, values); }

        public static T CreateInstance<T>(this Type typeRef)
        {
            try { return (T) Activator.CreateInstance(typeRef); } catch (Exception e)
            {
                var referenceException = e;

                while (referenceException.InnerException!= null) referenceException = referenceException.InnerException;

                throw referenceException;
            }
        }

        public static object CreateInstance(this Type typeRef)
        {
            try { return Activator.CreateInstance(typeRef); } catch (Exception e)
            {
                var referenceException = e;

                while (referenceException.InnerException!= null) referenceException = referenceException.InnerException;

                throw referenceException;
            }
        }

        public static ModelDescriptor ToModelDescriptor(this Type referenceType)
        {
            if (CachedInstances.ModelDescriptors.ContainsKey(referenceType)) return CachedInstances.ModelDescriptors[referenceType];

            var optionalTableAttribute = (TableAttribute) referenceType.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault();
            var targetTableName = optionalTableAttribute?.Name ?? referenceType.Name;

            var responseModelDescriptor = new ModelDescriptor {Collection = {SourceName = referenceType.Name, TargetName = targetTableName}, Members = new Dictionary<string, ModelDescriptor.MemberDescriptor>()};

            referenceType.GetProperties().ToList()
                .ForEach(property =>
                {
                    var optionalPropertyColumnAttribute = (ColumnAttribute) property.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault();
                    var name = optionalPropertyColumnAttribute?.Name ?? property.Name;

                    responseModelDescriptor.Members.Add(property.Name, new ModelDescriptor.MemberDescriptor {TargetName = name, SourceName = property.Name, ValueType = property.PropertyType, MemberSourceType = ModelDescriptor.MemberDescriptor.EMemberSourceType.Property});
                });

            referenceType.GetFields().ToList()
                .ForEach(member =>
                {
                    var optionalFieldColumnAttribute = (ColumnAttribute) member.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault();
                    var name = optionalFieldColumnAttribute?.Name ?? member.Name;

                    responseModelDescriptor.Members.Add(member.Name, new ModelDescriptor.MemberDescriptor {TargetName = name, SourceName = member.Name, ValueType = member.FieldType, MemberSourceType = ModelDescriptor.MemberDescriptor.EMemberSourceType.Field});
                });

            CachedInstances.ModelDescriptors[referenceType] = responseModelDescriptor;

            return responseModelDescriptor;
        }
    }
}