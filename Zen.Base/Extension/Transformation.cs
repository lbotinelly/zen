using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using ByteSizeLib;
using Newtonsoft.Json.Linq;
using Zen.Base.Common;
using Zen.Base.Module;

namespace Zen.Base.Extension
{
    public static class Transformation
    {
        public enum ESafeArrayMode
        {
            Remove,
            Allow
        }

        private static readonly Random Rnd = new Random();

        public static List<string> BlackListedModules = new List<string>
        {
            "System.Linq.Enumerable",
            "System.Collections.Generic.List",
            "System.Data.Common.DbCommand",
            "Oracle.DataAccess.Client.OracleCommand",
            "Dapper.SqlMapper+<QueryImpl>",
            "System.Web.Http.Controllers",
            "System.Runtime.CompilerServices",
            "System.Runtime.ExceptionServices",
            "CommonLanguageRuntimeLibrary"
        };

        public static T XmlToType<T>(this string path)
        {
            var serializer = new XmlSerializer(typeof(T));

            using var reader = new StreamReader(path);
            return (T)serializer.Deserialize(reader);
        }

        public static T XmlToType<T>(this Stream path)
        {
            var serializer = new XmlSerializer(typeof(T));

            using var reader = new StreamReader(path);
            return (T)serializer.Deserialize(reader);
        }

        private static T ConvertTo<T>(this XmlNode node) where T : class
        {
            var stm = new MemoryStream();

            var stw = new StreamWriter(stm);
            stw.Write(node.OuterXml);
            stw.Flush();

            stm.Position = 0;

            var ser = new XmlSerializer(typeof(T));
            var result = ser.Deserialize(stm) as T;

            return result;
        }

        // https://stackoverflow.com/a/33223183/1845714
        public static TV Val<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default) => dict != null && dict.TryGetValue(key, out var value) ? value : defaultValue;

        // https://stackoverflow.com/a/33223183/1845714
        public static TO Get<TK, TV, TO>(this IDictionary<TK, TV> dict, TK key, TO defaultValue = default) => dict != null && dict.TryGetValue(key, out var value) ? (TO)(object)value : defaultValue;

        public static TO Get<TO, TK, TV>(this IDictionary<TK, TV> dict, TK key)
        {
            TO defaultValue = default;
            // https://stackoverflow.com/a/33223183/1845714
            return dict != null && dict.TryGetValue(key, out var value) ? (TO)(object)value : defaultValue;
        }

        public static TD GetValueAs<TK, TV, TD>(this IDictionary<TK, TV> dict, TK key, TD type) => dict.Val(key).ToType<TD, TV>();

        public static string ToByteSize(this long size) => ByteSize.FromBytes(size).ToString();

        public static string ToByteSize(this int size) => ByteSize.FromBytes(size).ToString();

        public static Dictionary<TU, List<T>> DistributeEvenly<T, TU>(this IEnumerable<T> source, IEnumerable<TU> containers)
        {
            var enumeratedSource = source.ToList();
            var enumeratedContainers = containers.ToList();

            var distributionMap = enumeratedContainers.ToList().ToDictionary(i => i, i => new List<T>());
            var containerCount = distributionMap.Count;

            var containerIndex = 0;

            foreach (var item in enumeratedSource)
            {
                distributionMap[enumeratedContainers[containerIndex]].Add(item);
                containerIndex = (containerIndex + 1) % containerCount;
            }

            return distributionMap;
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            // "LINQ's Distinct() on a particular property"
            // https://stackoverflow.com/a/489421/1845714

            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
                if (seenKeys.Add(keySelector(element)))
                    yield return element;
        }

        public static string FileWildcardToRegex(string pattern) => "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";

        // https://stackoverflow.com/a/1833128/1845714
        public static T ToType<T>(this object value) => (T)ChangeType(typeof(T), value);

        public static object ChangeType(Type t, object value)
        {
            var tc = TypeDescriptor.GetConverter(t);
            return tc.ConvertFrom(value);
        }

        public static void RegisterTypeConverter<T, TC>() where TC : TypeConverter
        {
            TypeDescriptor.AddAttributes(typeof(T), new TypeConverterAttribute(typeof(TC)));
        }

        public static TU ToType<TU, T>(this T source) => source.ToJson().FromJson<TU>();
        public static T AsType<T>(this object source) => source.ToJson().FromJson<T>();

        public static void CopyProperties<T>(this T source, T destination)
        {
            if (source == null) return;

            // https://stackoverflow.com/a/7262846/1845714

            // Iterate the Properties of the destination instance and  
            // populate them from their source counterparts  
            var destinationProperties = destination.GetType().GetProperties();
            foreach (var destinationPi in destinationProperties)
            {
                var sourcePi = source.GetType().GetProperty(destinationPi.Name);
                try
                {
                    destinationPi.SetValue(destination, sourcePi?.GetValue(source, null), null);
                }
                catch (Exception) { }
            }
        }

        //https://stackoverflow.com/a/7029464/1845714
        public static DateTime RoundUp(this DateTime dt, TimeSpan d) => new DateTime((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks, dt.Kind);

        public static string ToOrdinal(this int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13: return num + "th";
            }

            switch (num % 10)
            {
                case 1: return num + "st";
                case 2: return num + "nd";
                case 3: return num + "rd";
                default: return num + "th";
            }
        }

        // public static MediaTypeNames.Image FromPathToImage(this string source) { return new Bitmap(source); }

        // https://stackoverflow.com/a/3284486/1845714
        public static DateTime Next(this DateTime date, DayOfWeek dayOfWeek) => date.AddDays((dayOfWeek < date.DayOfWeek ? 7 : 0) + dayOfWeek - date.DayOfWeek);

        public static void CopyValues<T>(this T source, T target, bool copyWhenSourceIsNull = false, bool copyWhenTargetIsNotNull = true)
        {
            var t = typeof(T);

            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source, null);
                var currValue = prop.GetValue(target, null);

                if (value == null && !copyWhenSourceIsNull) continue;
                if (currValue == null || copyWhenTargetIsNotNull) prop.SetValue(target, value, null);
            }
        }

        // https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa
        public static string ToHex(this byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba) hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static List<string> GetTemplateKeys(this string body)
        {
            if (body == null) return null;

            const string pattern = @"{{(.*?)}}";
            var matches = Regex.Matches(body, pattern).Select(m => m.Value).ToList();
            return matches;
        }

        public static string TemplateFill(this string body, object sourceObj)
        {
            var tmp = body;

            var source = JToken.Parse(sourceObj.ToJson());

            var keys = body.GetTemplateKeys();

            foreach (var key in keys)
            {
                var tokenName = key.Substring(2, key.Length - 4);
                var probe = source.SelectToken(tokenName);
                if (probe != null) tmp = tmp.Replace(key, probe.ToString());
            }

            return tmp;
        }

        public static string StripHtml(this string input) => input == null ? null : Regex.Replace(input, "<.*?>", string.Empty);

        public static IEnumerable<T> ToInstances<T>(this IEnumerable<Type> source)
        {
            return source.Select(i => (T)Activator.CreateInstance(i, new object[] { })).ToList();
        }

        public static T ToInstance<T>(this Type source)
        {
            return (T)Activator.CreateInstance(source, new object[] { });
        }

        public static IEnumerable<List<T>> SplitList<T>(List<T> items, int nSize = 30)
        {
            // https://stackoverflow.com/questions/11463734/split-a-list-into-smaller-lists-of-n-size

            for (var i = 0; i < items.Count; i += nSize) yield return items.GetRange(i, Math.Min(nSize, items.Count - i));
        }

        public static bool MatchWildcardPattern(this string value, string pattern)
        {
            var isMatch = Regex.IsMatch(value, pattern.WildCardToRegular());
            return isMatch;
        }

        public static T Random<T>(this IEnumerable<T> source)
        {
            if (source == null) return default;

            var enumerable = source.ToList();

            var r = Rnd.Next(enumerable.Count());
            return enumerable[r];
        }

        private static string WildCardToRegular(this string value) => "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";

        public static string ToQueryString(this Dictionary<string, string> obj)
        {
            var properties = from p in obj
                             where p.Value != null
                             select p.Key + "=" + HttpUtility.UrlEncode(p.Value);

            return string.Join("&", properties.ToArray());
        }

        public static string ToQueryString(this object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             where p.GetValue(obj, null) != null
                             select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null).ToString());

            return string.Join("&", properties.ToArray());
        }

        public static string SafeArray(this string source, string criteria = "", string keySeparator = "=", string elementSeparator = ",", ESafeArrayMode mode = ESafeArrayMode.Remove)
        {
            if (source == null) return "";
            var lineCol = source.Split(elementSeparator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            var criteriaCol = criteria.ToLower().Split(',').ToList();
            var ret = new List<string>();
            var compiledRet = "";

            foreach (var i in lineCol)
            {
                var item = i.Split(keySeparator.ToCharArray());

                var key = item[0].Trim().ToLower();

                var allow = false;

                switch (mode)
                {
                    case ESafeArrayMode.Remove:
                        allow = !criteriaCol.Contains(key);
                        break;
                    case ESafeArrayMode.Allow:
                        allow = criteriaCol.Contains(key);
                        break;
                }

                if (allow) ret.Add(i);
            }

            foreach (var item in ret)
            {
                if (compiledRet != "") compiledRet += elementSeparator;
                compiledRet += item;
            }

            return compiledRet;
        }

        public static string ToMd5b62Hash(this string input)
        {
            if (input == null) return null;

            using (var md5Hash = MD5.Create())
            {
                var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                return ToBase62String(data);
            }
        }

        public static string ToBase62String(this byte[] toConvert, bool bigEndian = false)
        {
            //https://codereview.stackexchange.com/questions/14084/base-36-encoding-of-a-byte-array

            const string characterSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            if (bigEndian) Array.Reverse(toConvert); // !BitConverter.IsLittleEndian might be an alternative
            var dividend = new BigInteger(toConvert);
            var builder = new StringBuilder();
            while (dividend != 0)
            {
                dividend = BigInteger.DivRem(dividend, 62, out var remainder);
                builder.Insert(0, characterSet[Math.Abs((int)remainder)]);
            }
            return builder.ToString();
        }

        public static string HashGuid(this string input, string salt = null)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input + salt));
                var guid = new Guid(hash).ToString("N");
                return guid;
            }
        }
        public static string HashGuid(this Stream inputStream)
        {
            using var md5 = MD5.Create();
            inputStream.Position = 0;
            var hash = md5.ComputeHash(inputStream);
            var guid = new Guid(hash).ToString("N");
            return guid;
        }

        public static bool IsAnyNullOrEmpty(params object[] objects)
        {
            foreach (var o in objects)
            {
                switch (o)
                {
                    case null: return true;
                    case string s: return string.IsNullOrEmpty(s);
                }

                return o.GetType().GetProperties()
                    .Any(x => IsNullOrEmpty(x.GetValue(o)));
            }

            return false;
        }

        private static bool IsNullOrEmpty(object value)
        {
            if (ReferenceEquals(value, null))
                return true;

            var type = value.GetType();
            return type.IsValueType && Equals(value, Activator.CreateInstance(type));
        }

        public static string Md5Hash(this string input, string salt = null)
        {
            if (input == null) return null;

            using (var md5Hash = MD5.Create())
            {
                var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input + salt));

                // Create a new Stringbuilder to collect the bytes and create a string.
                var sBuilder = new StringBuilder();

                //format each byte as hexadecimal 
                foreach (var b in data) sBuilder.Append(b.ToString("x2"));

                return sBuilder.ToString();
            }
        }

        // https://stackoverflow.com/a/5665784/1845714
        public static string Sha512Hash(this string input, string salt = null)
        {
            if (input == null) return null;

            using (var hash = SHA512.Create())
            {
                var data = hash.ComputeHash(Encoding.UTF8.GetBytes(input + salt));

                // Create a new Stringbuilder to collect the bytes and create a string.
                var sBuilder = new StringBuilder();

                //format each byte as hexadecimal 

                foreach (var b in data) sBuilder.Append(b.ToString("x2"));

                return sBuilder.ToString();
            }
        }

        public static string MetaHash(this string input, string salt = null)
        {
            var p1 = input.Md5Hash(salt);
            var p2 = input.Sha512Hash(salt);

            var p3 = p1 != null && p2 != null ? "-" : null;

            return p1 + p3 + p2;
        }

        // https://weblogs.asp.net/haithamkhedre/generate-guid-from-any-string-using-c
        public static Guid ToGuid(this string value)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            var md5Hasher = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
            return new Guid(data);
        }

        public static bool MD5HashCheck(this string input, string hash)
        {
            // Hash the input. 
            var hashOfInput = Md5Hash(input);

            // Create a StringComparer an compare the hashes.
            var comparer = StringComparer.OrdinalIgnoreCase;

            return 0 == comparer.Compare(hashOfInput, hash);
        }

        //public static string Encrypt(this string value) { return Current.Encryption.Encrypt(value); }
        //public static string Decrypt(this string value) { return Current.Encryption.Decrypt(value); }
        public static string Truncate(this string value, int maxChars) => value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";

        public static bool IsNumeric(this object refObj)
        {
            if (refObj == null) return false;

            long n;
            var isNumeric = long.TryParse(refObj.ToString(), out n);
            return isNumeric;
        }

        public static IEnumerable<string> SplitInChunksUpTo(this string str, int maxChunkSize)
        {
            for (var i = 0; i < str.Length; i += maxChunkSize) yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }

        public static ShortGuid ToShortGuid(this Guid oRef) => new ShortGuid(oRef);
        public static string FlatExceptionMessage(this Exception source) => FancyString(source);
        public static string FancyString(this Exception source) => new StackTrace(source, true).FancyString();

        public static string ToSummary(this Exception ex)
        {
            var output = "";

            output += ex.Message + new StackTrace(ex, true).FancyString();
            if (ex.InnerException != null) output += "; " + ex.InnerException.ToSummary();

            return output;
        }

        public static string FancyString(this StackTrace source)
        {
            var ret = "";

            var stFrames = source.GetFrames();

            if (stFrames == null) return null;

            var validFrames = stFrames.ToList();

            validFrames.Reverse();

            var mon = new Dictionary<string, string>();

            mon["mod"] = "";
            mon["type"] = "";

            var probe = "";

            foreach (var vf in validFrames)
            {
                if (vf.GetMethod().ReflectedType == null) continue;

                probe = vf.GetMethod().ReflectedType.FullName;
                if (BlackListedModules.Any(s => probe.IndexOf(s, StringComparison.OrdinalIgnoreCase) != -1)) continue;

                if (ret != "") ret += " > ";

                if (mon["mod"] == "")
                {
                    mon["mod"] = vf.GetMethod().Module.ToString();
                    ret += mon["mod"] + " - ";
                }

                if (mon["type"] != probe)
                {
                    mon["type"] = probe;
                    ret += vf.GetMethod().ReflectedType.FullName + ":";
                }

                ret += vf.GetMethod().Name;

                if (vf.GetFileColumnNumber() != 0) ret += "[L{1} C{0}]".format(vf.GetFileColumnNumber(), vf.GetFileLineNumber());
            }

            return ret;
        }

        public static string TrimSql(this string s) =>
            s
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ")
                .Replace("  ", " ")
                .Replace("  ", " ")
                .Replace("  ", " ")
                .Replace("  ", " ")
                .Replace("  ", " ")
                .Replace("  ", " ")
                .Replace("  ", " ");

        public static IDictionary<string, object> AddProperty(this object obj, string name, object value)
        {
            var dictionary = obj.ToMemberDictionary();
            dictionary.Add(name, value);
            return dictionary;
        }

        public static ICollection<T> ToCollection<T>(this List<T> items) where T : class
        {
            var ret = new Collection<T>();
            foreach (var t in items) ret.Add(t);
            return ret;
        }

        public static T? ToNullable<T>(this string s) where T : struct
        {
            var result = new T?();
            try
            {
                if (!string.IsNullOrEmpty(s) && s.Trim().Length > 0)
                {
                    var conv = TypeDescriptor.GetConverter(typeof(T));
                    result = (T)conv.ConvertFrom(s);
                }
            }
            catch { }

            return result;
        }

        public static T? ToNullable<T>(this long s) where T : struct
        {
            var result = new T?();
            try
            {
                var conv = TypeDescriptor.GetConverter(typeof(T));
                result = (T)conv.ConvertFrom(s);
            }
            catch { }

            return result;
        }

        public static IDictionary<string, object> AddPrefix(this Dictionary<string, object> source, string memberPrefix)
        {
            var ret = new Dictionary<string, object>();
            foreach (var (key, value) in source) ret.Add(memberPrefix + key, value);
            return ret;
        }


        public static IDictionary<string, object> ToMemberDictionary(this object obj, string memberPrefix = null)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();

            var type = obj.GetType();
            foreach (var item in type.GetProperties()) result.Add(memberPrefix + item.Name, item.GetValue(obj));
            foreach (var item in type.GetFields().Where(i=> i.IsPublic)) result.Add(memberPrefix + item.Name, item.GetValue(obj));
            return result;
        }

        static bool IsNullable<T>(T obj)
        {
            if (obj == null) return true; // obvious
            Type type = typeof(T);
            if (!type.IsValueType) return true; // ref-type
            if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
            return false; // value-type
        }

        public static bool IsNullable(this Type type)
        {
            if (!type.IsValueType) return true; // ref-type
            if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
            return false; // value-type
        }

        public static string ReplaceIn(this IDictionary<string, object> map, string source)
        {
            var res = new StringBuilder(source);

            foreach (var (key, value) in map)
            {
                if (value != null)
                    res.Replace("{" + key + "}", value.ToString());
            }

            return res.ToString();
        }

        public static T ToObject<T>(this IDictionary<string, object> source)
            where T : class, new()
        {
            var someObject = new T();
            var someObjectType = someObject.GetType();

            foreach (var item in source) someObjectType.GetProperty(item.Key).SetValue(someObject, item.Value, null);

            return someObject;
        }

        public static string ToCommaSeparatedString(this List<string> obj)
        {
            return obj.Aggregate((i, j) => i + ", " + j);
        }

        public static string format(this string source, params object[] parms) => string.Format(source, parms);

        public static List<List<T>> Split<T>(this List<T> items, int sliceSize = 30)
        {
            var list = new List<List<T>>();
            for (var i = 0; i < items.Count; i += sliceSize) list.Add(items.GetRange(i, Math.Min(sliceSize, items.Count - i)));
            return list;
        }

        public static bool TryCast<T>(ref T t, object o)
        {
            if (!(o is T)) return false;

            t = (T)o;
            return true;
        }

        public static T ConvertTo<T>(ref object input) => (T)Convert.ChangeType(input, typeof(T));

        public static object ToConcrete<T>(this ExpandoObject dynObject)
        {
            object instance = Activator.CreateInstance<T>();
            var dict = dynObject as IDictionary<string, object>;
            var targetProperties = instance.GetType().GetProperties();

            foreach (var property in targetProperties)
            {
                object propVal;
                if (dict.TryGetValue(property.Name, out propVal)) property.SetValue(instance, propVal, null);
            }

            return instance;
        }

        public static ExpandoObject ToExpando(this object staticObject)
        {
            var expando = new ExpandoObject();
            var dict = expando as IDictionary<string, object>;
            var properties = staticObject.GetType().GetProperties();

            foreach (var property in properties) dict[property.Name] = property.GetValue(staticObject, null);

            return expando;
        }

        public static bool StringsAreSimilar(string baseStr, string compareTo)
        {
            if (baseStr == compareTo) return true;

            var s1Words = baseStr.Split(' ');
            var s2Words = compareTo.Split(' ');

            if (s1Words.Length != s2Words.Length) return false;

            //This is needed to protect against typos and inconsistencies in data such as grill vs grille
            for (var i = 0; i < s1Words.Length; i++)
                try
                {
                    if (s1Words[i].SoundEx() != s2Words[i].SoundEx()) return false;
                }
                catch
                {
                    return false;
                }

            return true;
        }

        private static string SoundEx(this string str)
        {
            var result = new StringBuilder();
            if (!string.IsNullOrEmpty(str))
            {
                string previousCode = "", currentCode = "", currentLetter = "";
                result.Append(str.Substring(0, 1));

                for (var i = 1; i < str.Length; i++)
                {
                    currentLetter = str.Substring(i, 1).ToLower();
                    currentCode = "";

                    if ("bfpv".IndexOf(currentLetter, StringComparison.Ordinal) > -1) currentCode = "1";
                    else if ("cgjkqsxz".IndexOf(currentLetter, StringComparison.Ordinal) > -1) currentCode = "2";
                    else if ("dt".IndexOf(currentLetter, StringComparison.Ordinal) > -1) currentCode = "3";
                    else if (currentLetter == "l") currentCode = "4";
                    else if ("mn".IndexOf(currentLetter, StringComparison.Ordinal) > -1) currentCode = "5";
                    else if (currentLetter == "r") currentCode = "6";

                    if (currentCode != previousCode) result.Append(currentCode);
                }
            }

            if (result.Length < 4) result.Append(new string('0', 4 - result.Length));

            return result.ToString().ToUpper();
        }

        public static bool IgnoreCaseContains(this IEnumerable<string> list, string lookupStr)
        {
            lookupStr = lookupStr.ToLower();
            foreach (var str in list)
            {
                var s = str.ToLower();
                if (s.Equals(lookupStr)) return true;
            }

            return false;
        }

        public static string ProperCase(this string str) => ToTitleCase(str);
        public static string ToTitleCase(this string str) => new CultureInfo("en-US", false).TextInfo.ToTitleCase(str);

        public static string ToString(this JValue val) => val.ToObject<string>();

        public static bool IsValidEmail(this string strIn)
        {
            if (string.IsNullOrEmpty(strIn)) return false;

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch
            {
                return false;
            }

            // Return true if strIn is in valid e-mail format.
            try
            {
                return Regex.IsMatch(strIn,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private static string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            var idn = new IdnMapping();

            var domainName = match.Groups[2].Value;
            domainName = idn.GetAscii(domainName);

            return match.Groups[1].Value + domainName;
        }

        public static string FromNumberStringToString(this string source, int numDec = 2)
        {
            string ret;

            if (source == null) return null;

            try
            {
                var num = Convert.ToDecimal(source);

                var patt = "0:#";

                if (numDec > 0) patt += "." + new string('#', numDec);

                ret = string.Format("{" + patt + "}", num);
            }
            catch (Exception)
            {
                ret = source;
            }

            return ret;
        }

        public static DataReference ToReference<T>(this Data<T> source) where T : Data<T> => new DataReference { Key = source.GetDataKey(), Display = source.GetDataDisplay() };
        public static DataReference<T> TypedReference<T>(this Data<T> source) where T : Data<T> => new DataReference<T> { Key = source.GetDataKey(), Display = source.GetDataDisplay() };

        public static IEnumerable<DataReference> ToReference<T>(this IEnumerable<Data<T>> source) where T : Data<T>
        {
            return source.Select(i => i.ToReference());
        }

        public static T ToData<T>(this DataReference source) where T : Data<T> => Data<T>.Get(source.Key);
    }
}