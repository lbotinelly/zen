using System;
using System.Collections.Generic;
using System.Linq;
using Zen.App.Communication;
using Zen.App.Provider;
using Zen.App.Provider.Application;
using Zen.App.Provider.Group;
using Zen.App.Provider.Person;
using Zen.Base.Extension;
using Zen.Base.Module.Log;

namespace Zen.App.Model.Orchestration
{
    public static class Extensions
    {
        public static ZenApplicationRepresentation ToRepresentation(this IZenApplication source)
        {
            var output = source.ToJson().FromJson<ZenApplicationRepresentation>();

            var groups = Current.Orchestrator.GroupsByApplication(output.Id);

            output.Groups = groups.Select(i =>
            {
                var groupAction = i.ToJson().FromJson<ZenGroupAction>();

                groupAction.People = Current.Orchestrator.PeopleByGroup(groupAction.Id)
                    .Select(j => j.ToJson().FromJson<ZenPersonAction>()).ToList();

                return groupAction;
            }).ToList();

            return output;
        }

        public static bool FromRepresentation(this IZenApplication source, ZenApplicationRepresentation definition)
        {
            var adminGroupCodes = new List<string> {"ADM", "DEV", "CUR"};
            var isAdmin = Current.Orchestrator.HasAnyPermissions("ADM");

            var changes = 0;

            var log = new Dictionary<string, List<KeyValuePair<IZenGroup, IZenPerson>>>();

            Base.Current.Log.Add(
                "Group Setting for [{0}] {1}: {1} groups".format(Current.Orchestrator.Application.Code, Current.Orchestrator.Application.Name,
                                                                 definition.Groups.Count), Message.EContentType.Info);

            foreach (var g in definition.Groups)
            {
                var go = Current.Orchestrator.GetGroupByCode(g.Code);

                if (go.ApplicationId != null) // It's Owned by the app.
                    if (adminGroupCodes.Any(a => go.Code.IndexOf("_" + a, StringComparison.Ordinal) != -1))
                        // Seems to be part of the Administrative groupset
                        if (!isAdmin) // But user isn't admin
                        {
                            Base.Current.Log.Add(
                                $"[{Current.Orchestrator.Person.Locator}] {Current.Orchestrator.Person.Name} is NOT authorized to change group [{go.Code}] {go.Name}.",
                                Message.EContentType.MoreInfo);
                            continue;
                        }

                Base.Current.Log.KeyValuePair($"[{go.Code}] {go.Name}", $"{g.People.Count} people", Message.EContentType.MoreInfo);

                foreach (var p in g.People)
                    if (p.Action != null)
                    {
                        var po = Current.Orchestrator.GetPersonByLocator(p.Locator);

                        var action = p.Action.ToLower().Trim();

                        if (!log.ContainsKey(action)) log[action] = new List<KeyValuePair<IZenGroup, IZenPerson>>();

                        log[action].Add(new KeyValuePair<IZenGroup, IZenPerson>(go, po));

                        if (action == "add")
                        {
                            go.AddPerson(po);
                            changes++;
                        }

                        if (action == "del")
                        {
                            go.RemovePerson(po);
                            changes++;
                        }
                    }
            }

            if (changes == 0) return true;

            var body = "The following changes were made:<br/><br/>";

            if (log.ContainsKey("add"))
                foreach (var item in log["add"])
                {
                    if (body != "") body += "<br/>";
                    body = body + "<b>{0}</b> was added to group <b>{1}</b>".format(item.Value.Name, item.Key.Name);
                }

            if (log.ContainsKey("del"))
                foreach (var item in log["del"])
                {
                    if (body != "") body += "<br/>";
                    body = body + "<b>{0}</b> was removed from group <b>{1}</b>".format(item.Value.Name, item.Key.Name);
                }

            var e = new Email
            {
                Title = "Changes in Application Access Control",
                Header = "Application notification",
                Content = body
            };

            e.SetSender(Current.Orchestrator.Person);
            e.AddTo(Current.Orchestrator.Person);

            var app = Current.Orchestrator.Application;

            e.AddTo(app.GetGroup("ADM"));
            e.AddTo(app.GetGroup("CUR"));

            e.Send();

            //var f = new BucknellNotification
            //{
            //    options =
            //    {
            //        body = Status.Person.Name + " made " + (changes == 1 ? "one change" : changes + " changes") + " to Application Access Control",
            //        actions =
            //        {
            //            new Options.Action(Status.ConfigInfo.URL, "Visit App"),
            //            new Options.Action(Status.ConfigInfo.URL + "#/framework/groups", "Users and Groups")
            //        }
            //    }
            //};

            //BucknellPushDispatcher.Notify(app.GetGroup("ADM"), f);

            return true;
        }
    }
}