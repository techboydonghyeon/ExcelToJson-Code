using System.Reflection;

namespace DHLib
{
    static class FieldInfoExtensions
    {
        public static bool HasCustomAttribute<T>(this FieldInfo field)
        {
            var type = typeof( T );
            return field.GetCustomAttribute( type ) != null;
        }
    }
}