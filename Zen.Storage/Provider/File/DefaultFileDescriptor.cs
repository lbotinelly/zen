using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Zen.App.Model.Audience;
using Zen.App.Model.Tag;
using Zen.Base.Common;

namespace Zen.Storage.Provider.File
{
    public class DefaultFileDescriptor : IFileDescriptor
    {
        #region Implementation of IStorageEntityDescriptor

        public string StorageName { get; set; }
        public string StoragePath { get; set; }
        public DateTime Creation { get; set; }
        public EStorageEntityType Type { get; set; }

        #endregion

        #region Implementation of IDataId

        public string Id { get; set; }

        #endregion

        #region Implementation of IDataLocator

        public string Locator { get; set; }

        #endregion

        #region Implementation of IFileDescriptor

        public string OriginalName { get; set; }
        public string MimeType { get; set; }
        public long FileSize { get; set; }
        public TagCollection Tags { get; set; }
        public AudienceDefinition Audience { get; set; }
        public IFileDescriptor GetNewInstance()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
