using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace DHLib
{
    public static class TypeExtensions
    {
        public static object Create(this Type type, params object[] args) =>
            Activator.CreateInstance( type, args );

        public static object Deserialize(this Type type, string value)
        {
            if ( type.IsEnum )
                return Enum.Parse( type, value );

            if ( value == null )
                return null;

            // https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.typedescriptor?view=net-5.0
            var converter = TypeDescriptor.GetConverter(type);
            if ( converter != null )
            {
                if ( converter.CanConvertFrom(value.GetType()) )
                    return converter.ConvertFrom( value );
            }

            return Convert.ChangeType( value, type );
        }

        public static bool HasCustomAttribute<T>(this Type type) where T : Attribute
        {
            var attr = type.GetCustomAttribute<T>();
            return attr != null;
        }

        public static bool HasCustomAttributes<T>(this Type type) where T : Attribute
        {
            var attr = type.GetCustomAttributes<T>();
            return attr.Any();
        }
    }
}
