using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Zen.Base.Extension;
using Zen.Base.Module;

namespace Zen.Base.Distributed
{
    public class Ticket : Data<Ticket>
    {
        public enum EStates
        {
            Initialized,
            Running,
            Stopped,
            Finished
        }

        public static Dictionary<EStates, string> DStates = new Dictionary<EStates, string>
        {
            {EStates.Initialized, "Initialized"},
            {EStates.Running, "Running"},
            {EStates.Stopped, "Stopped"},
            {EStates.Finished, "Finished"}
        };

        public Ticket()
        {
            InstanceId = Status.InstanceId;
            State = DStates[EStates.Initialized];
        }

        [Key]
        public string Id { get; set; }
        [Display]
        public string ServiceDescriptor { get; set; }
        public string InstanceId { get; set; }
        public string State { get; set; }
        public string Comments { get; set; }
        public DateTime TimeOut { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public bool IsLocal() { return InstanceId == Status.InstanceId; }

        public bool CanRun()
        {
            Current.Log.KeyValuePair("State", State);
            return IsLocal() && State == DStates[EStates.Initialized];
        }

        public override void AfterSave(string newKey)
        {
            var currentModel = Get(newKey);
            var log = new TicketLog();

            currentModel.CopyPropertiesTo(log);
            log.TicketId = currentModel.Id;
            log.Id = Guid.NewGuid().ToString();
            log.Save();
        }

        public void Finish(string comment = "Finished")
        {
            State = DStates[EStates.Finished];
            Close(comment);
        }

        public void Start(string comment = "Started")
        {
            State = DStates[EStates.Running];
            Comment(comment);
        }

        public void Stop(string comment = "Stopped")
        {
            State = DStates[EStates.Stopped];
            Close(comment);
        }

        public void Comment(string comment)
        {
            // Current.Log.Add(this.ToJson());

            Comments = comment;
            Save();
        }

        public void Close(string comment = "Closed")
        {
            Comments = comment;
            var id = Save();

            // Current.Log.Add(this.ToJson());

            Remove(id.Id);
        }

        #region Overrides of Data<Ticket>

        public override void AfterInsert(string newKey) { Current.Log.KeyValuePair("Distributed Ticket Created", $"{Id} | {ServiceDescriptor}"); }

        public override void AfterRemove() { Current.Log.KeyValuePair("Distributed Ticket Disposed", $"{Id} | {ServiceDescriptor}"); }

        #endregion
    }
}