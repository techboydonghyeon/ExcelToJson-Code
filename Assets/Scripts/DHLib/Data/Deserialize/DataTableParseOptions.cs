using System;
using System.Collections.Generic;

namespace DHLib
{
    public class DataTableParseOptions
    {
        public delegate object ParseValue( string value );

        Dictionary<Type, ParseValue> mParseTableValue;

        public DataTableParseOptions()
        {
            mParseTableValue = new Dictionary<Type, ParseValue>();
        }

        public void SetCustomParser<T>(ParseValue customParser)
        {
            mParseTableValue.Add( typeof( T ), customParser );
        }

        public bool HasCustomParse(Type type)
        {
            return mParseTableValue.ContainsKey( type );
        }

        public object Parse(Type type, string value)
        {
            if ( mParseTableValue.TryGetValue( type, out var parseTableValue ) )
                return parseTableValue( value );

            return null;
        }
    }
}