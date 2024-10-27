using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;

namespace Zen.Service.Maintenance.Model
{
    [DataConfig(UseCaching = false)]
    public class Result : Data<Result>
    {
        public enum EResultStatus
        {
            Success,
            Undefined,
            Failed,
            Warning,
            Skipped
        }

        [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

        [Display] public string Message { get; set; } = "Success";

        public EResultStatus Status { get; set; } = EResultStatus.Success;
        public ConcurrentDictionary<string, long> Counters { get; set; } = new ConcurrentDictionary<string, long>();
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
        public int Priority { get; set; }

        public List<Change> Changes { get; set; } = new List<Change>();

        public DebugInfoBlock DebugInfo { get; set; } = new DebugInfoBlock();

        public Change AddChange(Change.EType type, string subject, string locator, string valueName = null,
            string originalValue = null, string newValue = null, string comments = null)
        {
            var entry = new Change
            {
                Comments = comments, Locator = locator, Subject = subject, Type = type,
                Value = new Change.ValueBlock {Name = valueName, From = originalValue, To = newValue}
            };
            Changes.Add(entry);
            return entry;
        }

        public void SetStep(string step)
        {
            Current.Log.Info(step);
            DebugInfo.Step = step;
        }

        public void SetDebugTarget(string target, Exception e = null)
        {
            Current.Log.Warn(target);
            DebugInfo.Target = target;

            if (e == null) return;

            Current.Log.Add(e);
            DebugInfo.TraceInfo = e.ToSummary();
        }

        public void Handle(Exception exception)
        {
            Status = EResultStatus.Failed;
            SetStep($"Failure handling: {exception.FancyString()}");
            Message = DebugInfo.Step;
        }

        public class Change
        {
            public enum EType
            {
                CREATE,
                MODIFY,
                REMOVE,
                OTHER
            }

            public string Subject { get; set; }
            public EType Type { get; set; }
            public string Locator { get; set; }
            public string Comments { get; set; }
            public ValueBlock Value { get; set; } = new ValueBlock();

            public class ValueBlock
            {
                public string Name { get; set; }
                public string From { get; set; }
                public string To { get; set; }
            }
        }

        public class DebugInfoBlock
        {
            public string Step { get; set; }
            public string TraceInfo { get; set; }
            public string Target { get; set; }
        }
    }
}