using System;
using Zen.App.Model.Audience;
using Zen.App.Model.Tag;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.Storage.Provider.File
{
    public interface IFileDescriptor : IStorageEntityDescriptor, IDataId, IDataLocator
    {
        string OriginalName { get; set; }
        string MimeType { get; set; }
        long FileSize { get; set; }
        TagCollection Tags { get; set; }
        AudienceDefinition Audience { get; set; }
        IFileDescriptor GetNewInstance();
    }

    public enum EStorageEntityType
    {
        Undefined,
        Collection,
        Item
    }

    public interface IStorageEntityDescriptor
    {
        string StorageName { get; set; }
        string StoragePath { get; set; }
        DateTime Creation { get; set; }
        EStorageEntityType Type { get; set; }
    }
}