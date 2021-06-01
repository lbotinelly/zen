namespace Zen.Web.Data.Pipeline.Moderation.Shared
{
    public class States
    {
        public enum EResult
        {
            Approved = 'A',
            Rejected = 'R',
            Withdrawn = 'W',
            TaskCreated = 'T'
        }

        public static class ResultLabel
        {
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string Withdrawn = "Withdrawn";
        }

        public class ModerationActions
        {
            public bool Author;
            public bool Moderate;
            public bool Whitelisted;

            public bool Read;
        }
    }
}