using System.ComponentModel.DataAnnotations;
using Zen.Base.Module;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.Media.Model
{
    public class MediaCacheData : Data<MediaCacheData>, IDataId
    {
        [Key]
        public string Id { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public bool Success { get; internal set; }
        public string Message { get; internal set; }
    }
}
