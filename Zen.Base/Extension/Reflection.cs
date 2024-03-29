﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace Zen.Base.Extension
{
    public static class Reflection
    {
        private static readonly Type[] PrimitiveTypes =
        {
            typeof(string),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(Guid),
            typeof(Enum),
            typeof(byte[]),
            typeof(object)
        };

        public static PropertyInfo[] GetPublicProperties(this Type type)
        {
            // https://stackoverflow.com/a/2444090/1845714

            if (type.IsInterface)
            {
                var propertyInfos = new List<PropertyInfo>();

                var considered = new List<Type>();
                var queue = new Queue<Type>();
                considered.Add(type);
                queue.Enqueue(type);
                while (queue.Count > 0)
                {
                    var subType = queue.Dequeue();
                    foreach (var subInterface in subType.GetInterfaces())
                    {
                        if (considered.Contains(subInterface)) continue;

                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }

                    var typeProperties = subType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);

                    var newPropertyInfos = typeProperties.Where(x => !propertyInfos.Contains(x));

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                return propertyInfos.ToArray();
            }

            return type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        ///     Gets an object fo type T, transposing matching keys from a reference dictionary. Optionally consumes a translation
        ///     dictionary that maps correlating keys.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict">The reference dictionary.</param>
        /// <param name="translationDictionary">The translation dictionary.</param>
        /// <returns></returns>
        public static T GetObject<T>(this IDictionary<string, object> dict, Dictionary<string, string> translationDictionary = null)
        {
            var type = typeof(T);

            var obj = Activator.CreateInstance(type);

            foreach (var kv in dict)
            {
                var propertyNameRes = kv.Key;

                if (translationDictionary!= null)
                    if (translationDictionary.ContainsValue(propertyNameRes))
                        propertyNameRes = translationDictionary.FirstOrDefault(x => x.Value == propertyNameRes).Key;

                var k = type.GetProperty(propertyNameRes,
                                         BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                var val = kv.Value;

                if (k == null) continue;

                var kt = k.PropertyType;

                if (k.PropertyType.IsPrimitiveType())
                {
                    if (val is decimal) val = Convert.ToInt64(val);
                    if (val is short && kt == typeof(bool)) val = Convert.ToInt16(val) == 1;
                    if (val is long && kt == typeof(string)) val = val.ToString();
                    if (kt == typeof(decimal)) val = Convert.ToDecimal(val);
                    if (kt == typeof(short)) val = Convert.ToInt16(val);
                    if (kt == typeof(int)) val = Convert.ToInt32(val);
                    if (kt == typeof(long)) val = Convert.ToInt64(val);
                    if (kt == typeof(Guid))
                        if (val!= null)
                            val = new Guid(val.ToString());
                    if (kt.IsEnum) val = Enum.Parse(k.PropertyType, val.ToString());

                    k.SetValue(obj, val);
                }

                else { k.SetValue(obj, kv.Value!= null ? JsonConvert.DeserializeObject(kv.Value.ToString(), kt) : null); }
            }

            return (T) obj;
        }

        public static IEnumerable<Type> GetParentTypes(this Type type)
        {
            // https://stackoverflow.com/a/18375526/1845714

            // is there any base type?
            if (type == null)
            {
                yield break;
            }

            // return all implemented or inherited interfaces
            foreach (var i in type.GetInterfaces())
            {
                yield return i;
            }

            // return all inherited types
            var currentBaseType = type.BaseType;
            while (currentBaseType != null)
            {
                yield return currentBaseType;
                currentBaseType = currentBaseType.BaseType;
            }
        }



        /// <summary>
        ///     Gets the method ext.
        /// </summary>
        /// <param name="thisType">Type of the this.</param>
        /// <param name="name">The name.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns></returns>
        public static MethodInfo GetMethodExt(this Type thisType, string name, params Type[] parameterTypes)
        {
            return GetMethodExt(thisType,
                                name,
                                BindingFlags.Instance
                                | BindingFlags.Static
                                | BindingFlags.Public
                                | BindingFlags.NonPublic
                                | BindingFlags.FlattenHierarchy,
                                parameterTypes);
        }

        /// <summary>
        ///     Gets the method ext.
        /// </summary>
        /// <param name="thisType">Type of the this.</param>
        /// <param name="name">The name.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns></returns>
        public static MethodInfo GetMethodExt(this Type thisType, string name, BindingFlags bindingFlags,
                                              params Type[] parameterTypes)
        {
            MethodInfo matchingMethod = null;

            // Check all methods with the specified name, including in base classes
            GetMethodExt(ref matchingMethod, thisType, name, bindingFlags, parameterTypes);

            // If we're searching an interface, we have to manually search base interfaces
            if (matchingMethod == null && thisType.IsInterface)
                foreach (var interfaceType in thisType.GetInterfaces())
                    GetMethodExt(ref matchingMethod,
                                 interfaceType,
                                 name,
                                 bindingFlags,
                                 parameterTypes);

            return matchingMethod;
        }

        private static void GetMethodExt(ref MethodInfo matchingMethod, Type type, string name,
                                         BindingFlags bindingFlags, params Type[] parameterTypes)
        {
            // Check all methods with the specified name, including in base classes
            foreach (var memberInfo in type.GetMember(name,
                                                      MemberTypes.Method,
                                                      bindingFlags))
            {
                var methodInfo = (MethodInfo) memberInfo;
                // Check that the parameter counts and types match, 
                // with 'loose' matching on generic parameters
                var parameterInfos = methodInfo.GetParameters();
                if (parameterInfos.Length == parameterTypes.Length)
                {
                    var i = 0;
                    for (; i < parameterInfos.Length; ++i)
                        if (!parameterInfos[i].ParameterType
                            .IsSimilarType(parameterTypes[i]))
                            break;
                    if (i == parameterInfos.Length)
                        if (matchingMethod == null) matchingMethod = methodInfo;
                        else
                            throw new AmbiguousMatchException(
                                "More than one matching method found!");
                }
            }
        }

        public static object GetPropertyValue(this object o, string property)
        {
            try
            {
                var propertyInfo = o.GetType().GetProperty(property);
                return propertyInfo?.GetValue(o);
            } catch { return null; }
        }

        public static object GetMemberValue(this object o, string member)
        {
            if (member == null) return null;
            if (o == null) return null;

            var targetType = o.GetType();

            // Member can be a path, like "People.Creator"
            var memberParts = member.Split('.');
            var currentMember = memberParts.FirstOrDefault();
            var nextMembers = memberParts.Skip(1).ToList();

            if (currentMember == null) return null;

            var probe = targetType.GetProperty(currentMember)?.GetValue(o) ?? targetType.GetField(currentMember)?.GetValue(o);

            if (!nextMembers.Any()) return probe;

            var partialPath = string.Join(".", nextMembers);

            return probe.GetMemberValue(partialPath);
        }

        public static bool HasMember(this object o, string member)
        {
            while (true)
            {
                var targetType = o.GetType();

                // Member can be a path, like "People.Creator"
                var memberParts = member.Split('.');
                var currentMember = memberParts.FirstOrDefault();
                var nextMembers = memberParts.Skip(1).ToList();

                if (currentMember == null) return false;

                var probe = targetType.GetProperty(currentMember)!= null || targetType.GetField(currentMember)!= null;

                if (!nextMembers.Any()) return probe;

                var partialPath = string.Join(".", nextMembers);

                o = o.GetMemberValue(currentMember);
                member = partialPath;
            }
        }

        public static bool SetMemberValue(this object source, string memberName, object model)
        {

            // Member can be a path, like "People.Creator"
            var memberParts = memberName.Split('.');
            var currentMember = memberParts.FirstOrDefault();
            var nextMembers = memberParts.Skip(1).ToList();

            if (!nextMembers.Any())
            {
                try
                {
                    // Case 1: Property

                    var propertyInfo = source.GetType().GetProperty(memberName);
                    if (propertyInfo!= null)
                    {
                        propertyInfo.SetValue(source, model, null);
                        return true;
                    }

                    // Case 2: Field

                    var fieldInfo = source.GetType().GetField(memberName);
                    if (fieldInfo == null) return false;
                    fieldInfo.SetValue(model, source);
                    return true;
                } catch { return false; }
            }

            var target = source.GetMemberValue(currentMember);
            var partialPath = string.Join(".", nextMembers);

            return target.SetMemberValue(partialPath, model);
        }


        public static T GetMemberValue<T>(this object o, string member) { return (T) GetMemberValue(o, member); }

        public static bool IsSimilarType(this Type thisType, Type type)
        {
            // Ignore any 'ref' types
            if (thisType.IsByRef) thisType = thisType.GetElementType();
            if (type.IsByRef) type = type.GetElementType();

            // Handle array types
            if (thisType.IsArray && type.IsArray) return thisType.GetElementType().IsSimilarType(type.GetElementType());

            // If the types are identical, or they're both generic parameters 
            // or the special 'T' type, treat as a match
            if (thisType == type || (thisType.IsGenericParameter || thisType == typeof(GetMethodExtT))
                && (type.IsGenericParameter || type == typeof(GetMethodExtT))) return true;

            // Handle any generic arguments
            if (thisType.IsGenericType && type.IsGenericType)
            {
                var thisArguments = thisType.GetGenericArguments();
                var arguments = type.GetGenericArguments();
                if (thisArguments.Length == arguments.Length) return !thisArguments.Where((t, i) => !t.IsSimilarType(arguments[i])).Any();
            }

            return false;
        }

        public static bool IsNumericType(object obj)
        {
            var numericTypes = new HashSet<Type>
            {
                typeof(byte),
                typeof(sbyte),
                typeof(ushort),
                typeof(uint),
                typeof(ulong),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(decimal),
                typeof(double),
                typeof(float)
            };
            return numericTypes.Contains(obj.GetType());
        }

        public static bool IsPrimitiveType(this Type type)
        {
            return
                type.IsValueType ||
                type.IsPrimitive ||
                PrimitiveTypes.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }

        public static IEnumerable<T> CreateInstances<T>(this IEnumerable<Type> typeRefs) { return typeRefs.Select(CreateInstance<T>); }

        public static T CreateInstance<T>(this Type typeRef)
        {
            try { return (T)Activator.CreateInstance(typeRef); }
            catch (Exception e)
            {
                var referenceException = e;

                while (referenceException.InnerException != null) referenceException = referenceException.InnerException;

                throw referenceException;
            }
        }

        public static T CreateGenericInstance<TU, T>(this Type typeRef)
        {
            try
            {
                var genericTypeRef = typeRef.MakeGenericType(typeof(TU));
                return (T)Activator.CreateInstance(genericTypeRef);
            }
            catch (Exception e)
            {
                var referenceException = e;

                while (referenceException.InnerException != null) referenceException = referenceException.InnerException;

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

        public static void CopyListPropertiesTo<T, TU>(this IEnumerable<T> source, List<TU> target, bool ignoreNull = false)
        {
            target ??= new List<TU>();

            target.Clear();

            foreach (var i in source)
            {
                var uo = (TU) Activator.CreateInstance(typeof(TU), null);

                i.CopyPropertiesTo(uo, ignoreNull);
                target.Add(uo);
            }
        }

        public static void CopyMembersTo<T>(this T source, T target, bool ignoreNull = false)
        {
            var type = typeof(T);
            foreach (var sourceProperty in type.GetProperties())
            {
                var targetProperty = type.GetProperty(sourceProperty.Name);

                var value = sourceProperty.GetValue(source, null);
                if (value == null) if (ignoreNull) continue;

                targetProperty.SetValue(target, value, null);
            }
            foreach (var sourceField in type.GetFields())
            {
                var targetField = type.GetField(sourceField.Name);

                var value = sourceField.GetValue(source);
                if (value == null) if (ignoreNull) continue;

                targetField.SetValue(target, value);
            }
        }

        public static void CopyPropertiesTo<T, TU>(this T source, TU target, bool ignoreNull = false)
        {
            var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead).ToList();
            var destProps = typeof(TU).GetProperties()
                .Where(x => x.CanWrite)
                .ToList();

            if (source == null) return;

            foreach (var sourceProp in sourceProps)
            {
                if (!destProps.Any(x => x.Name == sourceProp.Name)) continue;

                var p = destProps.First(x => x.Name == sourceProp.Name);

                var val = sourceProp.GetValue(source, null);

                if ((val == null) & ignoreNull) continue;

                var set = false;

                try
                {
                    p.SetValue(target, val, null);
                    set = true;
                } catch { }

                if (!set)
                    try
                    {
                        p.SetValue(target, val.ToString(), null);
                        set = true;
                    } catch (Exception) { }
            }
        }

        public class GetMethodExtT { }
    }
}