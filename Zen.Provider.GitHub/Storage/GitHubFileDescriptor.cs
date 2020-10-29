using System;
using Zen.App.Model.Audience;
using Zen.App.Model.Tag;
using Zen.Storage.Provider.File;

namespace Zen.Provider.GitHub.Storage {
    public class GitHubFileDescriptor : IFileDescriptor
    {
        public string Id { get; set; }
        public string Locator { get; set; }
        public string StorageName { get; set; }
        public string OriginalName { get; set; }
        public string StoragePath { get; set; }
        public string MimeType { get; set; }
        public long FileSize { get; set; }
        public DateTime Creation { get; set; }
        public EStorageEntityType Type { get; set; } = EStorageEntityType.Item;
        public TagCollection Tags { get; set; }
        public AudienceDefinition Audience { get; set; }
        public IFileDescriptor GetNewInstance() { throw new NotImplementedException(); }
        public override string ToString() => $"[F] {StorageName} ({StoragePath})";
    }

    public class GitHubDirectoryDescriptor: IStorageEntityDescriptor
    {
        public string StorageName { get; set; }
        public string StoragePath { get; set; }
        public DateTime Creation { get; set; }
        public EStorageEntityType Type { get; set; } = EStorageEntityType.Collection;
        public override string ToString() => $"[D] {StorageName} ({StoragePath})";
    }
}