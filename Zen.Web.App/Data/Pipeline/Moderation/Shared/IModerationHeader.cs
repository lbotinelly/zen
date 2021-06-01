using System;

namespace Zen.Web.Data.Pipeline.Moderation.Shared
{
    public interface IModerationHeader
    {
        string Id { get; set; }
        string Action { get; set; }
        string AuthorLocator { get; set; }
        string ModeratorLocator { get; set; }
        string SourceId { get; set; }
        DateTime TimeStamp { get; set; }
        string Rationale { get; set; }
    }
}