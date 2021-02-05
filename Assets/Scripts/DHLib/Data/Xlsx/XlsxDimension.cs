namespace DHLib
{
    public struct XlsxDimension
    {
        public int RowPivot;
        public int ColPivot;
        public int RowCount;
        public int ColCount;

        internal XlsxDimension(string reference)
        {
            var parts = reference.Split(':');
            if ( parts.Length == 2 )
            {
                var pivot = ParseReference( parts[0] );
                var count = ParseReference( parts[1] );
                RowPivot = pivot.row;
                ColPivot = pivot.col;
                RowCount = count.row;
                ColCount = count.col;
            }
            else
            {
                var pivot = ParseReference( parts[0] );
                RowPivot = 1;
                ColPivot = 1;
                RowCount = pivot.row;
                ColCount = pivot.col;
            }
        }

        public static (int row, int col) ParseReference(string value)
        {
            // Ex: A27
            const int offset = 'A' - 1;
            int row = 0;
            int col = 0;
            var position = 0;

            if ( value != null )
            {
                while ( position < value.Length )
                {
                    var c = value[position];
                    if ( c >= 'A' && c <= 'Z' )
                    {
                        position++;
                        col *= 26;
                        col += c - offset;
                        continue;
                    }

                    if ( char.IsDigit( c ) )
                        break;

                    position = 0;
                    break;
                }
            }

            row = int.Parse( value.Substring( position ) );
            return (row, col);
        }
    }
}
