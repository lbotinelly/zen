using System;
using System.ComponentModel.DataAnnotations;
using Zen.App.Model.Audience;
using Zen.App.Model.Tag;
using Zen.Base.Common;
using Zen.Storage.Provider.File;

namespace Zen.Storage.Model
{
    [Priority(Level = -99)]
    public class ZenFile : ZenFileBaseDescriptor<ZenFile>, IFileDescriptor {
        #region Implementation of IDataId
        [Key]
        public string Id { get; set; }

        #endregion

        #region Implementation of IDataLocator

        public string Locator { get; set; }

        #endregion

        #region Implementation of IZenFileDescriptor

        public string StorageName { get; set; }
        public string OriginalName { get; set; }
        public string StoragePath { get; set; }
        public string MimeType { get; set; }
        public long FileSize { get; set; }
        public DateTime Creation { get; set; } = DateTime.Now;
        public EStorageEntityType Type { get; set; } = EStorageEntityType.Item;
        public TagCollection Tags { get; set; }
        public AudienceDefinition Audience { get; set; }
        public IFileDescriptor GetNewInstance() => new ZenFile();

        #endregion
    }
}