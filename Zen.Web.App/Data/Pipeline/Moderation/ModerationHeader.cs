using System;
using Zen.Web.Data.Pipeline.Moderation.Shared;

namespace Zen.Web.App.Data.Pipeline.Moderation
{
    public class ModerationHeader : IModerationHeader
    {
        public string Id { get; set; }
        public string Action { get; set; }
        public string AuthorLocator { get; set; }
        public string ModeratorLocator { get; set; }
        public string SourceId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Rationale { get; set; }
    }
}