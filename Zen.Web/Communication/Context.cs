using System;
using System.Linq;
using Zen.App.Core.Person;
using Zen.Base.Module.Cache;

namespace Zen.Web.Communication
{
    public static class Context
    {
        public enum StreamCutoutType
        {
            Import,
            Stream,
            Mailing,
            Collection,
            Active,
            None
        }

        public static string NoUserLocator = "$$BucknellAuthorizationProvider_NOUSER";

        public static FilterPackage GetPersonFilterPackage(string locator)
        {
            if (locator == null) locator = NoUserLocator;

            return CacheFactory.FetchModel(InternalGetPersonFilterPackage, locator);
        }

        private static FilterPackage InternalGetPersonFilterPackage(string locator)
        {
            IPerson person = null;
            var ret = new FilterPackage();

            if (locator == NoUserLocator) return ret;

            ret.isUserPresent = true;
            person = App.Current.Orchestrator.GetPersonByLocator(locator);
            // First, Groups:
            var grpParm = string.Join(",", person.Groups() // Get all groups the Person belongs to
                                          .Select(i => "'" + i.Code + "'") // Add single quotes around each entry
            );

            // Then permissions:
            var perPrmList = person.Permissions;

            var prmParm = string.Join(",", perPrmList // Get all groups the Person belongs to
                                          .Select(i => "'" + i + "'") // Add single quotes around each entry
            );

            ret.Person = "'" + locator + "'";
            ret.Groups = $"{{$in:[{grpParm}]}}";
            ret.Permissions = $"{{$in:[{prmParm}]}}";

            return ret;
        }

        private static string GetPeriodStatement(DateTime? referenceDay = null)
        {
            if (referenceDay == null) referenceDay = DateTime.Now;
            var refDate = (DateTime) referenceDay;
            var strDate = refDate.ToString("o");

            var init = $"{{'Period.StartTime': {{$lte:ISODate(\"{strDate}\")}}}},{{'Period.StartTime': null}}";
            var end = $"{{'Period.EndTime': {{$gte:ISODate(\"{strDate}\")}}}},{{'Period.EndTime': null}}";

            return "{$or:[" + init + "]}, {$or:[" + end + "]}";
        }

        public static string GetQueryByContext(QueryByContextParm queryParameters)
        {
            Base.Current.Log.Add($"GetQueryByContext - locator:{queryParameters.Locator}, type: {queryParameters.Type}, streamType: {queryParameters.StreamType}, referenceDateTime: {queryParameters.ReferenceDateTime}");

            var package = GetPersonFilterPackage(queryParameters.Locator);

            var typeLim = "";

            var userStatement = package.GetStatement();
            if (userStatement!= null) userStatement = $",{{$or:{userStatement}}}";

            // ReSharper disable once InconsistentNaming
            var ANDlist = (queryParameters.ReferenceDateTime.HasValue ? $"{GetPeriodStatement(queryParameters.ReferenceDateTime.Value)}" : "") +
                          typeLim +
                          userStatement;

            var q = (queryParameters.WrapOutput ? "{" : "") +
                    (queryParameters.Active.HasValue ? $"Active: {queryParameters.Active.Value.ToString().ToLower()}, " : "") +
                    (queryParameters.Status!= null ? $"Status: '{queryParameters.Status}', " : "") +
                    (queryParameters.Type == null ? "" : $"Type: \'{queryParameters.Type}\', ") +
                    (ANDlist == "" ? "" : "$and:[" + ANDlist + "]") +
                    (queryParameters.WrapOutput ? "}" : "");

            return q;
        }

        public class QueryByContextParm
        {
            public bool? Active = true;
            public DateTime? ReferenceDateTime = null;
            public string Status = "Approved";
            public StreamCutoutType StreamType = StreamCutoutType.Stream;
            public string Type = null;
            public bool WrapOutput = true;
            public string Locator { get; set; } = App.Current.Orchestrator.Person?.Locator;
        }

        public class FilterPackage
        {
            public bool isUserPresent;
            public string Person { get; set; }
            public string Groups { get; set; }
            public string Permissions { get; set; }
            public string Tags { get; set; }

            public string GetStatement()
            {
                if (!isUserPresent) return null;

                // First case: Nothing specified, so entry is 'public'.
                var qSequence = "{'Audience.People': { '$size' : 0}, 'Audience.Groups': { '$size' : 0}, 'Audience.Permissions': { '$size' : 0}}";

                if (Person!= null)
                {
                    qSequence += qSequence != "" ? "," : "";
                    qSequence += $"{{'Audience.People': {Person}}}";
                }

                if (Groups!= null)
                {
                    qSequence += qSequence != "" ? "," : "";
                    qSequence += $"{{'Audience.Groups': {Groups}}}";
                }

                if (Permissions!= null)
                {
                    qSequence += qSequence != "" ? "," : "";
                    qSequence += $"{{'Audience.Permissions': {Permissions}}}";
                }

                if (Tags!= null)
                {
                    qSequence += qSequence != "" ? "," : "";
                    qSequence += $"{{'Audience.Tags': {Tags}}}";
                }

                return "[" + qSequence + "]";
            }
        }
    }
}