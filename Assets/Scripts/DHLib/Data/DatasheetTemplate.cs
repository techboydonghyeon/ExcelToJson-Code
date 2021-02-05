namespace DHLib
{
    static class DatasheetTemplate
    {
        public static void Initialize()
        {
            Datasheet.SetCustomParser<CSVInt>( ParseCSVInt );
            Datasheet.SetCustomParser<CSVFloat>( ParseCSVFloat );
            Datasheet.SetCustomParser<CSVString>( ParseCSVString );
            Datasheet.SetCustomParser<RangeInt>( ParseRangeInt );
            Datasheet.SetCustomParser<RangeFloat>( ParseRangeFloat );
        }

        public static object ParseCSVString(string value)
        {
            if ( value.IsEmpty() )
                return default;

            var values = value.Split( ',' );

            for ( int i = 0; i < values.Length; ++i )
                values[i] = values[i].Trim();

            return new CSVString() { Values = values };
        }

        public static object ParseCSVInt(string value)
        {
            if ( value.IsEmpty() )
                return default;

            var values = value.Split( ',' );
            var array = new int[values.Length];

            for ( int i = 0; i < values.Length; ++i )
                array[i] = int.Parse( values[i].Trim() );

            return new CSVInt() { Values = array };
        }

        public static object ParseCSVFloat(string value)
        {
            if ( value.IsEmpty() )
                return default;

            var values = value.Split( ',' );
            var array = new float[values.Length];

            for ( int i = 0; i < values.Length; ++i )
                array[i] = float.Parse( values[i].Trim() );

            return new CSVFloat() { Values = array };
        }

        public static object ParseRangeInt(string value)
        {
            if ( value.IsEmpty() )
                return default;

            if ( value.Length == 1 )
            {
                int single = int.Parse( value.Trim() );
                return new RangeInt() { Min = single, Max = single };
            }

            var values = value.Split( '~' );

            var min = int.Parse( values[0].Trim() );
            var max = int.Parse( values[1].Trim() );

            return new RangeInt() { Min = min, Max = max };
        }

        public static object ParseRangeFloat(string value)
        {
            if ( value.IsEmpty() )
                return default;

            if ( value.Length == 1 )
            {
                float single = float.Parse( value.Trim() );
                return new RangeFloat() { Min = single, Max = single };
            }

            var values = value.Split( '~' );

            var min = float.Parse( values[0].Trim() );
            var max = float.Parse( values[1].Trim() );

            return new RangeFloat() { Min = min, Max = max };
        }
    }
}
