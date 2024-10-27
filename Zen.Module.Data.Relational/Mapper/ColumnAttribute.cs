using Zen.Base.Module.Data;

namespace Zen.Module.Data.Relational.Mapper
{
    public sealed class ColumnAttribute : MemberAttribute
    {
        public long Length { get; set; }
        public bool Serialized { get; set; }
    }
}