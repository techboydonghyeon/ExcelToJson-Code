using System;
using System.Collections.Generic;

namespace DHLib
{
    public static class StringFormatter
    {
        public delegate string TokenValueFormat(string value, int precision);

        static readonly Dictionary<string, TokenValueFormat> mFormatter = new Dictionary<string, TokenValueFormat>()
        {
            { "F", Float },
            { "N", Int }
        };

        static string Float(string value, int precision)
        {
            return Convert.ToSingle( value ).ToString( "F" + precision );
        }

        static string Int(string value, int precision)
        {
            return Convert.ToInt32( value ).ToString( "N0" );
        }

    }
}