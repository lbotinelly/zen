using System;
using Zen.App.Model.Audience;
using Zen.App.Model.Tag;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.Storage.Provider.File
{
    public interface IFileDescriptor : IDataId, IDataLocator
    {
        string StorageName { get; set; }
        string OriginalName { get; set; }
        string StoragePath { get; set; }
        string MimeType { get; set; }
        long FileSize { get; set; }
        DateTime Creation { get; set; }
        TagCollection Tags { get; set; }
        AudienceDefinition Audience { get; set; }
        IFileDescriptor GetNewInstance();
    }
}