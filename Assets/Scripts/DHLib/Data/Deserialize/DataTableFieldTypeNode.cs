using System;
using System.Reflection;

namespace DHLib
{
    public class DataTableFieldTypeNode
    {
        public int Row { get; }
        public FieldInfo Field { get; }
        public DataTableFieldTypeNode Next { get; private set; }

        FieldTypeId mTypeId;
        int mReadLength;

        public DataTableFieldTypeNode(int row, FieldInfo field, FieldTypeId id)
        {
            Row = row;
            Field = field;
            mTypeId = id;
        }

        public void LinkTo(DataTableFieldTypeNode node)
        {
            Next = node;
            OnNext( node.Row );
        }

        public void OnNext(int row)
        {
            if ( mTypeId != FieldTypeId.None )
                mReadLength = row - Row;
        }

        public void SetValue(object obj, int column, DataTable table)
        {
            var value = ReadTable( column, table );
            Field.SetValue( obj, value );
        }

        object ReadTable(int column, DataTable table)
        {
            var value = table.GetValue( Row, column );
            var type = Field.FieldType;

            switch ( mTypeId )
            {
                case FieldTypeId.Array:
                    return ReadArrayField( column, table, type );
                case FieldTypeId.Object:
                    return ReadObjectField( type, value );
            }
            return null;
        }

        object ReadArrayField(int column, DataTable table, Type type)
        {
            var array = (Array)type.Create( mReadLength );
            var elementType = type.GetElementType();

            for ( int i = 0; i < mReadLength; ++i )
            {
                var row = table.GetValue( Row + i, column );
                var value = elementType.Deserialize( row );
                array.SetValue( value, i );
            }

            return array;
        }

        object ReadObjectField(Type type, string value)
        {
            if ( Datasheet.Options.HasCustomParse( type ) )
                return Datasheet.Options.Parse( type, value );

            return type.Deserialize( value );
        }
    }
}
