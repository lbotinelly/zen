using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace Zen.Base.Extension
{
    public static class Serialization
    {
        private static readonly object OLock = new object();

        public static void ThreadSafeAdd<T>(this List<T> source, T obj)
        {
            lock (OLock)
            {
                source.Add(obj);
            }
        }

        public static string MergeJson(this string source, string extra)
        {
            var sourceObject = JObject.Parse(source);
            var extraObject = JObject.Parse(extra);

            sourceObject.Merge(extraObject, new JsonMergeSettings {MergeArrayHandling = MergeArrayHandling.Union});

            source = sourceObject.ToString(Formatting.None);

            return source;
        }

        public static string ToXml(this object obj)
        {
            var serializer = new XmlSerializer(obj.GetType());

            using (var writer = new Utf8StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings {Indent = false}))
                {
                    var ns = new XmlSerializerNamespaces();

                    ns.Add("", "");

                    serializer.Serialize(xmlWriter, obj, ns);
                    return writer.ToString();
                }
            }
        }

        public static byte[] GetBytes(this string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(this byte[] bytes)
        {
            var chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public static string FromXmlToJson(this string obj)
        {
            var doc = new XmlDocument();
            doc.LoadXml(obj);
            return JsonConvert.SerializeXmlNode(doc);
        }

        public static string GetJsonNode(this string obj, string nodeName)
        {
            var jo = JObject.Parse(obj);
            var myTest = jo.Descendants()
                .Where(t => t.Type == JTokenType.Property && ((JProperty) t).Name == nodeName)
                .Select(p => ((JProperty) p).Value)
                .FirstOrDefault();
            return myTest.ToString();
        }

        public static string ToJson(this DataRow obj, List<string> columns = null)
        {
            var sb = new StringBuilder();

            if (columns == null) columns = (from DataColumn col in obj.Table.Columns select col.ColumnName).ToList();

            var colpos = 0;

            sb.Append("{");

            foreach (var column in columns)
            {
                if (obj[colpos].ToString() != "")
                {
                    if (colpos > 0) sb.Append(", ");
                    sb.Append("\"" + column + "\":");

                    if (obj[colpos].GetType().Name == "DateTime") sb.Append("\"" + ((DateTime) obj[colpos]).ToString("o") + "\"");
                    else sb.Append(CleanupJsonData(obj[colpos]));
                }

                colpos++;
            }

            sb.Append("}");

            return sb.ToString();
        }

        public static string ToJson(this DataTable obj)
        {
            var sb = new StringBuilder();
            sb.Append("[");

            var idx = 0;
            var columns = (from DataColumn cols in obj.Columns select cols.ColumnName).ToList();

            foreach (DataRow row in obj.Rows)
            {
                if (idx > 0) sb.Append(",");

                sb.Append(row.ToJson(columns));
                idx++;
            }

            sb.Append("]");

            return sb.ToString();
        }

        public static string ToJson(this DataTableReader obj)
        {
            var sb = new StringBuilder();
            sb.Append("[");

            var isFirstRow = true;

            while (obj.Read())
            {
                if (!isFirstRow) sb.Append(",");

                sb.Append("{");
                for (var i = 0; i < obj.FieldCount; i++)
                {
                    if (obj.GetValue(i) == null) continue;
                    if (obj.GetValue(i) is DBNull) continue;

                    if (i > 0) sb.Append(", ");

                    sb.Append("\"" + obj.GetName(i) + "\":");

                    if (obj.GetValue(i).GetType().Name == "DateTime") sb.Append("\"" + obj.GetDateTime(i).ToString("o") + "\"");
                    else sb.Append(CleanupJsonData(obj.GetValue(i).ToString()));
                }

                sb.Append("}");
                isFirstRow = false;
            }

            sb.Append("]");
            return sb.ToString();
        }

        public static string ToQueryString(this object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                where p.GetValue(obj, null) != null
                select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null).ToString());

            return string.Join("&", properties.ToArray());
        }

        public static string ToFriendlyUrl(this string title)
        {
            if (title == null) return "";

            const int maxlen = 80;
            var len = title.Length;
            var prevdash = false;
            var sb = new StringBuilder(len);
            char c;

            for (var i = 0; i < len; i++)
            {
                c = title[i];
                if (c >= 'a' && c <= 'z' || c >= '0' && c <= '9')
                {
                    sb.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // tricky way to convert to lowercase
                    sb.Append((char) (c | 32));
                    prevdash = false;
                }
                else if (c == ' ' || c == ',' || c == '.' || c == '/' ||
                         c == '\\' || c == '-' || c == '_' || c == '=')
                {
                    if (!prevdash && sb.Length > 0)
                    {
                        sb.Append('-');
                        prevdash = true;
                    }
                }
                else if (c >= 128)
                {
                    var prevlen = sb.Length;
                    sb.Append(RemapInternationalCharToAscii(c));
                    if (prevlen != sb.Length) prevdash = false;
                }

                if (i == maxlen) break;
            }

            if (prevdash) return sb.ToString().Substring(0, sb.Length - 1);
            return sb.ToString();
        }

        public static string RemapInternationalCharToAscii(char c)
        {
            var s = c.ToString().ToLowerInvariant();
            if ("àåáâäãåą".Contains(s)) return "a";
            if ("èéêëę".Contains(s)) return "e";
            if ("ìíîïı".Contains(s)) return "i";
            if ("òóôõöøőð".Contains(s)) return "o";
            if ("ùúûüŭů".Contains(s)) return "u";
            if ("çćčĉ".Contains(s)) return "c";
            if ("żźž".Contains(s)) return "z";
            if ("śşšŝ".Contains(s)) return "s";
            if ("ñń".Contains(s)) return "n";
            if ("ýÿ".Contains(s)) return "y";
            if ("ğĝ".Contains(s)) return "g";
            if (c == 'ř') return "r";
            if (c == 'ł') return "l";
            if (c == 'đ') return "d";
            if (c == 'ß') return "ss";
            if (c == 'Þ') return "th";
            if (c == 'ĥ') return "h";
            if (c == 'ĵ') return "j";
            return "";
        }

        // ReSharper disable once InconsistentNaming
        public static string ToISODateString(this DateTime obj, bool includeLocalTimezone = true) =>
            !includeLocalTimezone
                ? $"ISODate(\"{obj:o}\")"
                : $"ISODate(\"{obj:o}{TimeZoneInfo.Local.BaseUtcOffset.Hours}:00\")";

        public static string ToISOString(this DateTime obj) => $"{obj:o}";

        // ReSharper disable once InconsistentNaming
        public static string ToRawDateHash(this DateTime obj) => obj.ToString("yyyyMMddHHmmss");

        public static DateTime FromRawDateHash(this string obj) => DateTime.ParseExact(obj, "yyyyMMddHHmmss", new CultureInfo("en-US"));

        // ReSharper disable once InconsistentNaming
        public static string ToFutureISODateString(this TimeSpan obj) => DateTime.Now.Add(obj).ToISODateString();

        // ReSharper disable once InconsistentNaming
        public static string ToPastISODateString(this TimeSpan obj) => DateTime.Now.Subtract(obj).ToISODateString();

        public static string ToBase64(this string obj) => obj == null ? null : Convert.ToBase64String(Encoding.UTF8.GetBytes(obj));

        public static string FromBase64(this string obj)
        {
            if (obj == null) return null;

            var data = Convert.FromBase64String(obj);
            var decodedString = Encoding.UTF8.GetString(data);

            return decodedString;
        }

        public static bool IsBasicType(this Type type)
        {
            // https://gist.github.com/jonathanconway/3330614

            return
                type.IsValueType ||
                type.IsPrimitive ||
                new[]
                {
                    typeof(string),
                    typeof(decimal),
                    typeof(string),
                    typeof(decimal),
                    typeof(DateTime),
                    // typeof(DateTimeOffset),
                    // typeof(TimeSpan),
                    typeof(Guid)
                }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }

        public static bool IsNullOrEmpty(this string source) => string.IsNullOrEmpty(source);

        public static string TruncateEnd(this string text, int length, bool padLeft = false)
        {
            if (string.IsNullOrEmpty(text)) text = new string(' ', length - 1);

            if (text.Length < length) return !padLeft ? text : text.PadLeft(length - 1);

            var rendered = "..." + text.Substring(Math.Max(0, text.Length - length + 4));

            return rendered;
        }

        public static ExpandoObject ToExpando(this IDictionary<string, object> dictionary)
        {
            // https://theburningmonk.com/2011/05/idictionarystring-object-to-expandoobject-extension-method/

            var expando = new ExpandoObject();
            var expandoDic = (IDictionary<string, object>) expando;

            // go through the items in the dictionary and copy over the key value pairs)
            foreach (var kvp in dictionary)
                // if the value can also be turned into an ExpandoObject, then do it!
                if (kvp.Value is IDictionary<string, object>)
                {
                    var expandoValue = ((IDictionary<string, object>) kvp.Value).ToExpando();
                    expandoDic.Add(kvp.Key, expandoValue);
                }
                else if (kvp.Value is ICollection)
                {
                    // iterate through the collection and convert any strin-object dictionaries
                    // along the way into expando objects
                    var itemList = new List<object>();
                    foreach (var item in (ICollection) kvp.Value)
                        if (item is IDictionary<string, object>)
                        {
                            var expandoItem = ((IDictionary<string, object>) item).ToExpando();
                            itemList.Add(expandoItem);
                        }
                        else
                        {
                            itemList.Add(item);
                        }

                    expandoDic.Add(kvp.Key, itemList);
                }
                else
                {
                    expandoDic.Add(kvp);
                }

            return expando;
        }

        public static List<X509Certificate2> ToList(this X509Certificate2Collection source) => source.OfType<X509Certificate2>().ToList();

        public static List<X509Certificate2> BySubject(this X509Store source, string targetSubject)
        {
            source.Open(OpenFlags.ReadOnly);
            var target = source.Certificates.ToList().Where(i => i?.SubjectName.Name != null && i.SubjectName.Name.Contains(targetSubject)).ToList();
            source.Close();
            return target;
        }

        public static void CopyStreamTo(this Stream input, Stream output)
        {
            var buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0) output.Write(buffer, 0, len);
        }

        public static bool IsJson(this string strInput)
        {
            // https://stackoverflow.com/a/14977915/1845714

            strInput = strInput.Trim();
            if ((!strInput.StartsWith("{") || !strInput.EndsWith("}")) && (!strInput.StartsWith("[") || !strInput.EndsWith("]"))) return false;

            try
            {
                JToken.Parse(strInput);
                return true;
            }
            catch (JsonReaderException jex)
            {
                //Exception in parsing json
                Console.WriteLine(jex.Message);
                return false;
            }
            catch (Exception ex) //some other exception
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public static string XmlToJson(this string source)
        {
            var doc = new XmlDocument();
            doc.LoadXml(source);

            var json = JsonConvert.SerializeXmlNode(doc, Formatting.Indented);

            return json;
        }

        public static string ToJson(this XElement source) => source.ToString().XmlToJson();

        public static List<T> ToList<T>(this IEnumerator<T> e)
        {
            var list = new List<T>();
            while (e.MoveNext()) list.Add(e.Current);
            return list;
        }

        public static int Years(this TimeSpan source) => (new DateTime(1, 1, 1) + source).Year - 1;

        public static string ContentToString(this Stream source)
        {
            var reader = new StreamReader(source);
            source.Position = 0;
            return reader.ReadToEnd();
        }

        public static string ConvertByOptionsMap(this Dictionary<string, List<string>> sourceMap, string originalText, bool returnOriginalIfMiss = false)
        {

            var result = returnOriginalIfMiss ? originalText : null;

            var (correctTerm, value) = sourceMap.FirstOrDefault(i => i.Value.Any(originalText.Contains));

            if (correctTerm == null) return result;

            foreach (var incorrectTerm in value.Where(originalText.Contains))
            {
                result = originalText.Replace(incorrectTerm, correctTerm);
                break;
            }

            return result;
        }
        public static void Ensure<T>(this List<T> collection, T item)
        {
            if (item == null) return;
            collection ??= new List<T>();

            var serializedItem = item.ToJson();

            if (collection.All(i => i.ToJson() != serializedItem)) collection.Add(item);
        }
        public static Dictionary<int, string> ToDictionary(this Enum source)
        {
            var et = source.GetType();
            return Enum.GetValues(et).Cast<int>().ToDictionary(i => i, i => Enum.GetName(et, i));
        }

        public static TU TryGet<T,TU>(this Dictionary<T, TU> source, T key)
        {
            if (source.ContainsKey(key)) return source[key];
            return default;
        }

        public static string ToJson(this object obj, int pLevels = 0, bool ignoreEmptyStructures = false, Formatting format = Formatting.None, bool enumToString = false)
        {
            //var s = new JavaScriptSerializer {MaxJsonLength = 50000000};
            //if (pLevels != 0) s.RecursionLimit = pLevels;
            //return s.Serialize(obj);

            var settings = new JsonSerializerSettings();

            if (ignoreEmptyStructures)
            {
                settings.NullValueHandling = NullValueHandling.Ignore;
                settings.DefaultValueHandling = DefaultValueHandling.Ignore;
                if (enumToString) settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            }

            try
            {
                return JsonConvert.SerializeObject(obj, format, settings);
            }
            catch
            {
                return null;
            }
        }

        public static JObject ToJObject(this object src) => JObject.Parse(src.ToJson());

        public static string CleanSqlFormatting(this string source)
        {
            var ret = source.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ").Trim();
            while (ret.IndexOf("  ", StringComparison.Ordinal) != -1) ret = ret.Replace("  ", " ");
            return ret;
        }

        public static IDictionary<string, object> ToKeyValueDictionary(this string source) => source.FromJson<IDictionary<string, object>>();

        public static string StrVal(this JObject source, string query) => source?.SelectTokens(query).FirstOrDefault()?.ToString().Trim();
        public static string StrVal(this JObject source, IEnumerable<string> querySet)
        {
            foreach (var query in querySet)
            {
                try
                {
                    var probe = source?.SelectTokens(query).FirstOrDefault()?.ToString().Trim();
                    if (probe != null) return probe;

                }
                catch (Exception)
                {
                    throw;
                }

            }

            return null;
        }
        public static JObject JValue(this JObject source, IEnumerable<string> querySet)
        {
            foreach (var query in querySet)
            {
                var probe = source?.SelectTokens(query).FirstOrDefault();
                if (probe != null) return (JObject)probe;

            }

            return null;
        }
        public static JObject JValues(this JObject source, IEnumerable<string> querySet)
        {
            foreach (var query in querySet)
            {
                var probe = source?.SelectTokens(query);
                if (probe != null) return (JObject)probe;

            }

            return null;
        }

        public static Dictionary<string, string> ToPathValueDictionary(this JObject source)
        {
            var ret = new Dictionary<string, string>();

            foreach (var jToken in (JToken) source)
            {
                var t = (JProperty) jToken;

                var k = t.Name;
                var v = t.Value;

                if (v is JObject) ret = ret.Concat(ToPathValueDictionary((JObject) v)).ToDictionary(x => x.Key, x => x.Value);
                else ret.Add(t.Path, v.ToString());
            }

            return ret;
        }

        public static T FromJson<T>(this string obj) => obj == null ? default : JsonConvert.DeserializeObject<T>(obj);

        public static object FromJson(this string obj, Type destinyFormat, bool asList)
        {
            var type = destinyFormat;

            if (asList)
            {
                var genericListType = typeof(List<>);

                var specificListType = genericListType.MakeGenericType(destinyFormat);
                type = ((IEnumerable<object>) Activator.CreateInstance(specificListType)).GetType();
            }

            if (obj == null) return null;
            return JsonConvert.DeserializeObject(obj, type);
        }

        public static dynamic ToObject(this Dictionary<string, object> source)
        {
            var eo = new ExpandoObject();
            var eoColl = (ICollection<KeyValuePair<string, object>>) eo;

            foreach (var kvp in source) eoColl.Add(kvp);

            dynamic eoDynamic = eo;

            return eoDynamic;
        }

        public static byte[] ToByteArray<T>(this T obj)
        {
            // https://stackoverflow.com/a/33022788/1845714

            if (obj == null) return null;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static byte[] ToByteArray(this Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                var readBuffer = new byte[4096];

                var totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        var nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            var temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte) nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                var buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }

                return buffer;
            }
            finally
            {
                if (stream.CanSeek) stream.Position = originalPosition;
            }
        }

        public static T FromByteArray<T>(this byte[] data)
        {
            // https://stackoverflow.com/a/33022788/1845714

            if (data == null) return default;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream(data))
            {
                var obj = bf.Deserialize(ms);
                return (T) obj;
            }
        }

        public static byte[] ToSerializedBytes(this object obj)
        {
            byte[] result;
            using (var stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, obj);
                stream.Flush();
                result = stream.ToArray();
            }

            return result;
        }

        public static T FromSerializedBytes<T>(this byte[] obj)
        {
            using (var stream = new MemoryStream(obj))
            {
                var ser = new BinaryFormatter();
                return (T) ser.Deserialize(stream);
            }
        }

        private static string CleanupJsonData(object data)
        {
            var ret = Reflection.IsNumericType(data)
                ? data.ToString()
                : "\"" +
                  data.ToString()
                      .Replace("\\", "\\\\")
                      .Replace("\"", "\\\"")
                      .Replace(Environment.NewLine, "\\n") +
                  "\"";

            return ret;
        }

        public static string GetString(this HttpWebResponse a)
        {
            var streamReader = new StreamReader(a.GetResponseStream(), true);
            try
            {
                return streamReader.ReadToEnd();
            }
            finally
            {
                streamReader.Close();
            }
        }

        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) return true;
                toCheck = toCheck.BaseType;
            }

            return false;
        }

        public static string CacheKey(this Type baseclass, string id = null, string fullNameAlias = null) => CacheKey(baseclass, id, fullNameAlias, null);

        public static string CacheKey(this Type baseclass, string id, string fullNameAlias, string suffix = null)
        {
            var basename = (fullNameAlias ?? baseclass.FullName) + suffix;

            return basename + (id == null ? "" : ":" + id);
        }

        public static bool IsBetween<T>(this T item, T start, T end) =>
            Comparer<T>.Default.Compare(item, start) >= 0
            && Comparer<T>.Default.Compare(item, end) <= 0;

        // https://stackoverflow.com/a/930599/1845714
        /// <summary>
        ///     Non-generic class allowing properties to be copied from one instance
        ///     to another existing instance of a potentially different type.
        /// </summary>
        /// <summary>
        ///     Copies all public, readable properties from the source object to the
        ///     target. The target type does not have to have a parameterless constructor,
        ///     as no new instance needs to be created.
        /// </summary>
        /// <remarks>
        ///     Only the properties of the source and target types themselves
        ///     are taken into account, regardless of the actual types of the arguments.
        /// </remarks>
        /// <typeparam name="TSource">Type of the source</typeparam>
        /// <typeparam name="TTarget">Type of the target</typeparam>
        /// <param name="source">Source to copy properties from</param>
        /// <param name="target">Target to copy properties to</param>
        public static void Copy<TSource, TTarget>(TSource source, TTarget target)
            where TSource : class
            where TTarget : class
        {
            PropertyCopier<TSource, TTarget>.Copy(source, target);
        }

        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;

            public override string NewLine => "";
        }
    }

    /// <summary>
    ///     Generic class which copies to its target type from a source
    ///     type specified in the Copy method. The types are specified
    ///     separately to take advantage of type inference on generic
    ///     method arguments.
    /// </summary>
    public static class PropertyCopy<TTarget> where TTarget : class, new()
    {
        /// <summary>
        ///     Copies all readable properties from the source to a new instance
        ///     of TTarget.
        /// </summary>
        public static TTarget CopyFrom<TSource>(TSource source) where TSource : class => PropertyCopier<TSource, TTarget>.Copy(source);
    }

    /// <summary>
    ///     Static class to efficiently store the compiled delegate which can
    ///     do the copying. We need a bit of work to ensure that exceptions are
    ///     appropriately propagated, as the exception is generated at type initialization
    ///     time, but we wish it to be thrown as an ArgumentException.
    ///     Note that this type we do not have a constructor constraint on TTarget, because
    ///     we only use the constructor when we use the form which creates a new instance.
    /// </summary>
    internal static class PropertyCopier<TSource, TTarget>
    {
        /// <summary>
        ///     Delegate to create a new instance of the target type given an instance of the
        ///     source type. This is a single delegate from an expression tree.
        /// </summary>
        private static readonly Func<TSource, TTarget> creator;

        /// <summary>
        ///     List of properties to grab values from. The corresponding targetProperties
        ///     list contains the same properties in the target type. Unfortunately we can't
        ///     use expression trees to do this, because we basically need a sequence of statements.
        ///     We could build a DynamicMethod, but that's significantly more work :) Please mail
        ///     me if you really need this...
        /// </summary>
        private static readonly List<PropertyInfo> sourceProperties = new List<PropertyInfo>();

        private static readonly List<PropertyInfo> targetProperties = new List<PropertyInfo>();
        private static readonly Exception initializationException;

        static PropertyCopier()
        {
            try
            {
                creator = BuildCreator();
                initializationException = null;
            }
            catch (Exception e)
            {
                creator = null;
                initializationException = e;
            }
        }

        internal static TTarget Copy(TSource source)
        {
            if (initializationException != null) throw initializationException;
            if (source == null) throw new ArgumentNullException("source");
            return creator(source);
        }

        internal static void Copy(TSource source, TTarget target)
        {
            if (initializationException != null) throw initializationException;
            if (source == null) throw new ArgumentNullException("source");
            for (var i = 0; i < sourceProperties.Count; i++) targetProperties[i].SetValue(target, sourceProperties[i].GetValue(source, null), null);
        }

        private static Func<TSource, TTarget> BuildCreator()
        {
            var sourceParameter = Expression.Parameter(typeof(TSource), "source");

            var bindings = new List<MemberBinding>();
            foreach (var sourceProperty in typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!sourceProperty.CanRead) continue;
                var targetProperty = typeof(TTarget).GetProperty(sourceProperty.Name);
                if (targetProperty == null) throw new ArgumentException("Property " + sourceProperty.Name + " is not present and accessible in " + typeof(TTarget).FullName);
                if (!targetProperty.CanWrite) throw new ArgumentException("Property " + sourceProperty.Name + " is not writable in " + typeof(TTarget).FullName);
                if ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0) throw new ArgumentException("Property " + sourceProperty.Name + " is static in " + typeof(TTarget).FullName);
                if (!targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType)) throw new ArgumentException("Property " + sourceProperty.Name + " has an incompatible type in " + typeof(TTarget).FullName);
                bindings.Add(Expression.Bind(targetProperty, Expression.Property(sourceParameter, sourceProperty)));
                sourceProperties.Add(sourceProperty);
                targetProperties.Add(targetProperty);
            }

            Expression initializer = Expression.MemberInit(Expression.New(typeof(TTarget)), bindings);
            return Expression.Lambda<Func<TSource, TTarget>>(initializer, sourceParameter).Compile();
        }
    }
}