using System;
using System.ComponentModel.DataAnnotations;
using Zen.Base.Extension;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.Base.Module
{
    public class KeyData<T> : Data<T>, IDataId where T : Data<T>
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToShortGuid();
    }
}