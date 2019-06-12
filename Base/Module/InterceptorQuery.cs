using System;
using System.Text;

namespace Zen.Base.Module
{
    public class InterceptorQuery
    {
        public enum EType
        {
            StaticArray
        }

        public enum EOperation
        {
            Query,
            Distinct,
            Update
        }
    }
}
