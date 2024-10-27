using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using Zen.App.Core.Application;
using Zen.App.Core.Group;
using Zen.App.Core.Person;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Log;

namespace Zen.App.Communication
{
    public class Email : Data<Email>
    {
        public enum ETemplateModel
        {
            Basic,
            Simple,
            Branded
        }

        private static readonly SmtpClient Smtp = new SmtpClient(Current.EmailConfiguration.SmtpServer);

        private readonly MailMessage _baseMsg = new MailMessage();
        internal IApplication Application;

        public AddressGroup Bcc = new AddressGroup {Code = "Bcc"};
        public AddressGroup Cc = new AddressGroup {Code = "Cc"};

        public EmailPreferences Preferences = new EmailPreferences();

        public EmailStatus Result = new EmailStatus();
        public SerializableMailAddress Sender;
        public AddressGroup To = new AddressGroup {Code = "To"};

        public Email()
        {
            if (Current.Orchestrator.Application != null) SetApplication(Current.Orchestrator.Application);
            if (Current.Orchestrator.Person != null) SetSender(Current.Orchestrator.Person);

            Bcc.SetParent(this);
            Cc.SetParent(this);
            To.SetParent(this);

            EnvironmentCode = Base.Current.Environment.Current.Code;
        }

        [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Header { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool Sent { get; set; }
        public DateTime CreationTimestamp { get; set; } = DateTime.Now;
        public string Campaign { get; set; }
        public string ApplicationId { get; private set; }
        public string SenderLocator { get; set; }
        public string EnvironmentCode { get; set; }
        public string RecipientList { get; set; }

        public string ApplicationCode { get; set; }

        public bool IgnoreEnvironment { get; set; }

        //public static Email FromMetadataTemplateKey(string templateKey)
        //{
        //    var tmpl = Status.MetaData.Value<string>(templateKey);
        //    return FromGuideId(tmpl);
        //}

        //public static Email FromGuideId(string guideId)
        //{
        //    var ret = new Email();

        //    // Temporarily setting the thread token as Service, so the Guide can be obtained even if the user normally wouldn't have access to it.

        //    string currStGuid = null;

        //    var apprGuideEntry = Guide.Get(guideId);
        //    var postTemplate = apprGuideEntry.Body;
        //    var finalBodytext = Markdown.ToHtml(postTemplate);

        //    ret.Content = finalBodytext;
        //    ret.Title = apprGuideEntry.Title;

        //    return ret;
        //}

        public Email FillTemplateKeys(object source)
        {
            Content = Content.TemplateFill(source);
            Title = Title.TemplateFill(source);

            return this;
        }

        public string GetCompiledRecipientList()
        {
            var ret = "";

            var col = new List<AddressGroup> {To, Cc};

            foreach (var i in col)
            foreach (var item in i.RecipientDescriptor)
            {
                if (ret != "") ret += ", ";

                var piece = item.Key;
                if (item.Value != null)
                    piece = "<a href=\"mailto:{0}\" target=\"_top\">{1}</a>".format(item.Value, item.Key);

                ret += piece;
            }

            return ret;
        }

        public static IEnumerable<Email> QueuedByCampaign(string campaignCode)
        {
            var q = new {Campaign = campaignCode, Sent = false};

            return Query(q.ToJson());
        }

        public Email AddAttachment(string name, string content)
        {
            _baseMsg.Attachments.Add(Attachment.CreateAttachmentFromString(content, name));
            return this;
        }

        public Email SetSender(IPerson person)
        {
            if (person?.Email == null || person.Name == null) return this;

            SenderLocator = person.Locator;
            Sender = (SerializableMailAddress) new MailAddress(person.Email, person.Name);

            return this;
        }

        private static IPerson LocatorStringToPerson(string email)
        {
            if (email == null) return null;

            email = email.ToLower().Trim();

            var parsedLocator = email;

            var person =
                Current.Orchestrator.GetPersonByEmail(parsedLocator) ??
                Current.Orchestrator.GetPersonByLocator(parsedLocator);

            return person?.Id != null ? person : null;

            // Zen.Base.Current.Log.Add("ERR Person not found: " + parsedLocator, Message.EContentType.Warning);
        }

        public Email SetSender(string email, string label = null)
        {
            if (email == null) return this;

            email = email.ToLower().Trim();

            var probe = LocatorStringToPerson(email);
            if (probe != null)
            {
                SetSender(probe);
                return this;
            }

            SenderLocator = email;
            Sender = (SerializableMailAddress) (label == null ? new MailAddress(email) : new MailAddress(email, label));

            return this;
        }

        public void Send(bool ignoreSent = false, bool ignoreEnvironment = false)
        {
            IgnoreEnvironment = ignoreEnvironment;

            var msg = new PreMail();

            if (Sent)
                if (!ignoreSent)
                {
                    Base.Current.Log.Add($"Email {Id} was already sent at {Result.Timestamp} ",
                        Message.EContentType.Warning);
                    return;
                }

            try
            {
                var isNotPrd = !Host.IsProduction;

                msg.From = Sender;

                var title = Title;

                if (!Preferences.SupressSubjectAppCodeTag)
                    title = "[" + Application.Code + (isNotPrd ? " - " + Base.Current.Environment.CurrentCode : "") +
                            "] " + title;

                msg.Subject = title;

                foreach (var i in To.AddressList) msg.To.Add(i);
                foreach (var i in Cc.AddressList) msg.CC.Add(i);
                foreach (var i in Bcc.AddressList) msg.Bcc.Add(i);

                var content = Current.EmailConfiguration.GetBodyTemplate();

                if (Header == null) Header = Title;

                content = content.Replace("{{Title}}", Title ?? "");
                content = content.Replace("{{Content}}", Content ?? "");
                content = content.Replace("{{Header}}", Header ?? "");
                content = content.Replace("{{TimeStamp}}", DateTime.Now.ToString(CultureInfo.InvariantCulture) ?? "");
                content = content.Replace("{{Content}}", Content ?? "");
                content = content.Replace("{{Environment}}", Content ?? "");

                if (Application != null)
                {
                    content = content.Replace("{{Application.Name}}", Application.Name ?? "");

                    if (Preferences.SupressSubjectAppCodeTag) content = content.Replace("{{Application.Code}}", "");

                    content = content.Replace("{{Application.Code}}", Application.Code ?? "");
                    //content = content.Replace("{{Application.Url}}", Current.Orchestrator.Application.GetConfigurationBlock()?.URL ?? "");
                }

                RecipientList = GetCompiledRecipientList();

                content = content.Replace("{{Sender.DisplayName}}", msg.From.DisplayName);
                content = content.Replace("{{Recipient.CompiledList}}", RecipientList);

                if (!ignoreEnvironment)
                    if (isNotPrd)
                    {
                        msg.To.Clear();
                        msg.CC.Clear();
                        msg.Bcc.Clear();

                        To.Clear();
                        Cc.Clear();
                        Bcc.Clear();

                        var devGroups = new List<IGroup> {Application.GetGroup("DEV"), Application.GetGroup("DEVCOPY")};

                        devGroups = devGroups.Where(i => i != null).ToList();

                        if (!devGroups.Any())
                        {
                            devGroups.Add(Application.GetGroup("ADM"));
                            devGroups = devGroups.Where(i => i != null).ToList();
                        }

                        if (devGroups.Any())
                        {
                            foreach (var a in devGroups)
                            {
                                AddTo(a);
                                foreach (var i in a.GetPeople().ToList())
                                    msg.To.Add(new SerializableMailAddress(i.Email, i.Name));
                            }
                        }
                        else
                        {
                            Base.Current.Log.Add(
                                "Non-PRD Environment can't send email: no DEV/DEVCOPY/ADM groups found.",
                                Message.EContentType.Warning);
                            AddTo(Current.EmailConfiguration.FallbackPersonEmail);

                            return;
                        }

                        Result.Message = "Non-PRD environment: Original recipients replaced by DEV list members.";
                    }

                //cleanup
                content = content.Replace("[]", "");

                msg.Body = content;
                msg.IsBodyHtml = true;

                // var targetCount = _msg.To.Count + _msg.CC.Count + _msg.Bcc.Count;
                var targetCount = msg.CC.Count + msg.Bcc.Count;

                Base.Current.Log.Add(
                    $"Sending message [{msg.Subject}] TO {targetCount} recipients FROM {msg.From.Address}",
                    Message.EContentType.Maintenance);

                // Finally, if single

                // Does it goes over the 100-recipient limit?

                const int maxBlockSize = 50;

                // if (targetCount > 0)
                if (targetCount > maxBlockSize)
                {
                    var targetCol = new List<Tuple<string, SerializableMailAddress>>();

                    //if (_msg.To.Count > 0)
                    //    foreach (var i in _msg.To)
                    //        targetCol.Add(new Tuple<string, SerializableMailAddress>("to", i));

                    if (msg.CC.Count > 0)
                        foreach (var i in msg.CC)
                            targetCol.Add(new Tuple<string, SerializableMailAddress>("cc", i));

                    if (msg.Bcc.Count > 0)
                        foreach (var i in msg.Bcc)
                            targetCol.Add(new Tuple<string, SerializableMailAddress>("bcc", i));

                    // Now send individual blocks.

                    var blockPointer = 0;
                    List<Tuple<string, SerializableMailAddress>> targetSet = null;

                    var baseMsg = msg.ToJson().FromJson<PreMail>();

                    // baseMsg.To.Clear();
                    baseMsg.CC.Clear();
                    baseMsg.Bcc.Clear();

                    var msgsToSend = new List<PreMail>();

                    do
                    {
                        var copyMsg = baseMsg.ToJson().FromJson<PreMail>();

                        targetSet = targetCol.Skip(blockPointer).Take(maxBlockSize).ToList();

                        if (targetSet.Count == 0) continue;

                        foreach (var t in targetSet)
                            switch (t.Item1)
                            {
                                //case "to":
                                //    copyMsg.To.Add(t.Item2);
                                //    break;
                                case "cc":
                                    copyMsg.CC.Add(t.Item2);
                                    break;
                                case "bcc":
                                    copyMsg.Bcc.Add(t.Item2);
                                    break;
                            }

                        msgsToSend.Add(copyMsg);
                        blockPointer += maxBlockSize;
                    } while (targetSet.Count > 0);

                    Base.Current.Log.Add(
                        $"    BlockSend: {msgsToSend.Count} (maxSize: {maxBlockSize} | total: {targetCol.Count})",
                        Message.EContentType.Maintenance);

                    foreach (var preMail in msgsToSend) Smtp.Send(preMail.ToMailMessage());
                }
                else
                {
                    Smtp.Send(msg.ToMailMessage(_baseMsg));
                }

                Result.Status = EmailStatus.EStatus.Sent;
                Result.Timestamp = DateTime.Now;
                Sent = true;

                Save();
            }
            catch (Exception e)
            {
                Result.Status = EmailStatus.EStatus.Error;
                Result.Timestamp = DateTime.Now;
                Result.Message = e.Message;

                Sent = false;

                Save();

                Base.Current.Log.Add("Could NOT send email: " + e.Message, Message.EContentType.Warning);
                Base.Current.Log.Add(e);
                throw;
            }
        }

        public void SetApplication(string code)
        {
            var targetApp = Current.Orchestrator.GetApplicationByCode(code);

            if (targetApp == null) throw new InvalidDataException("No application found for Code " + code);

            SetApplication(targetApp);
        }

        public void SetApplication(IApplication app)
        {
            Application = app;
            ApplicationCode = app.Code;
            ApplicationId = app.Id;
        }

        /// <summary>
        ///     Automatically creates a new instance of Email pre-populated with a list of recipient Groups
        /// </summary>
        /// <param name="groupList">The Group list</param>
        /// <returns></returns>
        public static Email ToGroup(string groupList)
        {
            var ret = new Email();
            ret.AddTo(groupList);
            if (Current.Orchestrator.Person != null) ret.SetSender(Current.Orchestrator.Person);
            return ret;
        }

        public class AddressGroup
        {
            private Email _parent;
            public List<SerializableMailAddress> AddressList = new List<SerializableMailAddress>();
            public Dictionary<string, string> Emails = new Dictionary<string, string>();
            public List<string> Groups = new List<string>();
            public Dictionary<string, string> RecipientDescriptor = new Dictionary<string, string>();
            internal string Code { get; set; }

            public void SetParent(Email reference)
            {
                _parent = reference;
            }

            public void Clear()
            {
                Groups.Clear();
                AddressList.Clear();
                Emails.Clear();
            }

            internal Email Add(string code, string displayName = null)
            {
                if (displayName == null) displayName = code;

                //Is it a list? If so, split and handle members individually.
                if (code.IndexOf(',') != -1)
                {
                    var items = code.Split(',');

                    foreach (var item in items) Add(item);

                    return _parent;
                }

                if (code.IndexOf(';') != -1)
                {
                    var items = code.Split(';');

                    foreach (var item in items) Add(item);

                    return _parent;
                }

                // First, lookup App groups.

                var appGroup = _parent.Application.GetGroup(code);

                if (appGroup != null)
                {
                    Add(appGroup);
                    return _parent;
                }

                // Then People.

                var person = Current.Orchestrator.GetPersonByLocator(code);

                if (person != null)
                {
                    Add(person);
                    return _parent;
                }

                // if it's a valid email, add it as just a string

                if (!code.IsValidEmail()) return _parent;

                if (!RecipientDescriptor.ContainsKey(code)) RecipientDescriptor.Add(displayName, code);
                AddressList.Add((SerializableMailAddress) new MailAddress(code, displayName));

                return _parent;
            }

            public Email Add(IGroup group)
            {
                if (group == null)
                {
                    Base.Current.Log.Add($"Email:{Code}: Group:NULL - No group specified",
                        Message.EContentType.Warning);
                    return _parent;
                }

                try
                {
                    AddGroupToCollection(group, ref Groups, ref Emails,
                        delegate(MailAddress address) { AddressList.Add((SerializableMailAddress) address); });
                    return _parent;
                }
                catch (Exception e)
                {
                    Base.Current.Log.Add(e, $"Email:{Code}:Group:{group.Code}");
                    return _parent;
                }
            }

            public Email Add(IPerson person)
            {
                try
                {
                    RecipientDescriptor.Add(person.Name, person.Email);
                    AddressList.Add((SerializableMailAddress) new MailAddress(person.Email, person.Name));
                    return _parent;
                }
                catch (Exception e)
                {
                    Base.Current.Log.Add(e, $"Email:{Code}: person:{person.Locator}");
                    return _parent;
                }
            }

            private void AddGroupToCollection(IGroup grp, ref List<string> groupList,
                ref Dictionary<string, string> groupEmails, Action<MailAddress> action)
            {
                if (!groupList.Contains(grp.Code))
                {
                    if (!RecipientDescriptor.ContainsKey(grp.Code))
                    {
                        var name = grp.Name;

                        if (grp.ApplicationId != null)
                        {
                            var probe = Current.Orchestrator.GetApplicationById(grp.ApplicationId);
                            if (probe != null) name = probe.Name + " - " + name;
                        }

                        if (!RecipientDescriptor.ContainsKey(name)) RecipientDescriptor.Add(name, null);
                    }

                    groupList.Add(grp.Code);
                }

                var ppl = grp.GetPeople();

                foreach (var item in ppl)
                {
                    if (groupEmails.ContainsKey(item.Email.ToLower())) continue;

                    groupEmails.Add(item.Email.ToLower(), item.Name);

                    action(new MailAddress(item.Email, item.Name));
                }
            }
        }

        public class PreMail
        {
            public SerializableMailAddress From { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public bool IsBodyHtml { get; set; }
            public List<SerializableMailAddress> To { get; set; } = new List<SerializableMailAddress>();
            public List<SerializableMailAddress> CC { get; set; } = new List<SerializableMailAddress>();
            public List<SerializableMailAddress> Bcc { get; set; } = new List<SerializableMailAddress>();

            public MailMessage ToMailMessage(MailMessage ret = null)
            {
                if (ret == null) ret = new MailMessage();

                ret.From = (MailAddress) From;
                ret.Subject = Subject;
                ret.Body = Body;
                ret.IsBodyHtml = IsBodyHtml;

                if (To != null)
                    foreach (var mailAddress in To)
                        ret.To.Add((MailAddress) mailAddress);

                if (CC != null)
                    foreach (var mailAddress in CC)
                        ret.CC.Add((MailAddress) mailAddress);

                if (Bcc != null)
                    foreach (var mailAddress in Bcc)
                        ret.Bcc.Add((MailAddress) mailAddress);

                return ret;
            }
        }

        public class EmailPreferences
        {
            public bool SupressSubjectAppCodeTag;

            public EmailPreferences()
            {
                if (!Host.IsDevelopment) SupressSubjectAppCodeTag = true; // Always suppress if coming from Production 
            }
        }

        public class EmailStatus
        {
            public enum EStatus
            {
                New,
                Sent,
                Error
            }

            public EStatus Status = EStatus.New;
            public DateTime? Timestamp;
            public string Message { get; set; }
        }

        public class SerializableMailAddress
        {
            public SerializableMailAddress()
            {
            }

            public SerializableMailAddress(string email, string name)
            {
                Address = email;
                DisplayName = name;
            }

            public string DisplayName { get; set; }
            public string User { get; set; }
            public string Host { get; set; }
            public string Address { get; set; }

            public static explicit operator MailAddress(SerializableMailAddress s)
            {
                return new MailAddress(s.Address, s.DisplayName);
            }

            public static explicit operator SerializableMailAddress(MailAddress b)
            {
                return new SerializableMailAddress
                {
                    Address = b.Address,
                    DisplayName = b.DisplayName,
                    Host = b.Host,
                    User = b.User
                };
            }
        }

        public static class Elements
        {
            public static string hr = "<hr style='background-color:#e7e7e7;border:none;color:#e7e7e7;height:1px;'>";
        }

        #region To

        public Email AddTo(IPerson person)
        {
            return To.Add(person);
        }

        public Email AddTo(IGroup group)
        {
            return To.Add(group);
        }

        public Email AddTo(string code)
        {
            return To.Add(code);
        }

        public Email AddTo(string code, string displayName)
        {
            return To.Add(code, displayName);
        }

        #endregion

        #region CC

        public void AddCc(IPerson person)
        {
            Cc.Add(person);
        }

        public void AddCc(IGroup group)
        {
            Cc.Add(group);
        }

        public void AddCc(string code)
        {
            Cc.Add(code);
        }

        public void AddCc(string code, string displayName)
        {
            Cc.Add(code, displayName);
        }

        #endregion

        #region BCC

        public void AddBcc(IPerson person)
        {
            Bcc.Add(person);
        }

        public void AddBcc(IGroup group)
        {
            Bcc.Add(group);
        }

        public void AddBcc(string code)
        {
            Bcc.Add(code);
        }

        public void AddBcc(string code, string displayName)
        {
            Bcc.Add(code, displayName);
        }

        #endregion
    }
}