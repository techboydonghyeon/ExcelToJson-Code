using System;
using System.Collections.Generic;

namespace DHLib
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrEmpty( value );
        }

        public static bool IsValid(this string value)
        {
            return !IsEmpty( value );
        }
    }
}