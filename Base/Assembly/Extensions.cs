using System;

namespace Zen.Base.Assembly
{
    public static class Extensions
    {
        public static T CreateInstance<T>(this Type typeRef)
        {
            try { return (T) Activator.CreateInstance(typeRef); } catch (Exception e)
            {
                var referenceException = e;

                while (referenceException.InnerException != null) referenceException = referenceException.InnerException;

                throw referenceException;
            }
        }
    }
}