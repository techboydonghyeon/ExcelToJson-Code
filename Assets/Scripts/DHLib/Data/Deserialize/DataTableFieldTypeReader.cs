using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DHLib
{
    public static class DataTableFieldTypeReader
    {
        public static object Read(Type type, DataTable table)
        {
            var id = ReadTypeId( type );
            switch ( id )
            {
                case FieldTypeId.Array:
                    var elementType = type.GetElementType();
                    return ReadArrayType( type, elementType, table );

                case FieldTypeId.IDictionary:
                    {
                        var genericTypes = type.GetGenericArguments();
                        var keyType = genericTypes[0];
                        var valueType = genericTypes[1];
                        return ReadDictionaryType( type, keyType, valueType, table );
                    }

                case FieldTypeId.IList:
                    {
                        var genericTypes = type.GetGenericArguments();
                        var valueType = genericTypes[0];
                        return ReadListType( type, valueType, table );
                    }

                case FieldTypeId.Object:
                    var node = CreateFieldTypeNode( type, table );
                    return ReadObjectType( type, 0, table, node );
            }

            throw new Exception();
        }

        static object ReadDictionaryType(Type type, Type keyType, Type valueType, DataTable table)
        {
            var node = CreateFieldTypeNode( valueType, table );
            var dictionary = (IDictionary)type.Create();
            for ( int i = 0; i < table.ValueCount;  ++i )
            {
                // node의 0번 인데스를 Key로 사용한다.
                var key = SelectKey( keyType, table, i );
                var value = ReadObjectType( valueType, i, table, node );
                dictionary.Add( key, value );
            }
            return dictionary;
        }

        static object SelectKey(Type keyType, DataTable table, int column)
        {
            var value = table.GetValue( 0, column );
            var obj = keyType.Deserialize( value );
            return obj;
        }

        static object ReadListType(Type type, Type valueType, DataTable table)
        {
            var node = CreateFieldTypeNode( valueType, table );
            var list = (IList)type.Create();
            for ( int i = 0; i < table.ValueCount; ++i )
            {
                var value = ReadObjectType( valueType, i, table, node );
                list.Add( value );
            }
            return list;
        }

        static object ReadArrayType(Type type, Type elementType, DataTable table)
        {
            var node = CreateFieldTypeNode( elementType, table );
            var array = (Array)type.Create();
            for ( int i = 0; i < table.ValueCount; ++i )
            {
                var value = ReadObjectType( elementType, i, table, node );
                array.SetValue( value, i );
            }

            return array;
        }

        static object ReadObjectType(Type type, int column, DataTable table, DataTableFieldTypeNode node)
        {
            var obj = type.Create();

            for ( ; node != null; node = node.Next )
                node.SetValue( obj, column, table: table );

            return obj;
        }

        static FieldTypeId ReadTypeId(Type type)
        {
            if ( type.IsArray )
                return FieldTypeId.Array;

            if ( type.IsGenericType )
            {
                Type genericTypeDef = type.GetGenericTypeDefinition();
                if ( genericTypeDef == typeof( List<> ) )
                    return FieldTypeId.IList;
                if ( genericTypeDef == typeof( Dictionary<,> ) )
                    return FieldTypeId.IDictionary;
            }

            return FieldTypeId.Object;
        }

        // ----------------------------------------------------------------
        // DataTableTypeNode Methods.
        // ----------------------------------------------------------------

        static DataTableFieldTypeNode CreateFieldTypeNode(Type type, DataTable source)
        {
            if ( source.MemberCount == 0 )
                return null;

            var flags = BindingFlags.Public | BindingFlags.Instance;
            var fields = type.GetFields( flags );
            int fieldRef = 1;

            var head = CreateNode( 0, fields[0] );
            var node = head;
            for ( int i = 1; i < source.MemberCount; ++i )
            {
                string value = source.GetMember(i);
                if ( IsCommentLine( value ) )
                    continue;

                var field = fields[fieldRef++];

                var next = CreateNode( i, field );
                node.LinkTo( next );
                node = next;
            }

            // Array의 길이를 측정하기 위해 추가.
            node.OnNext( source.MemberCount );
            return head;
        }

        static bool IsCommentLine(string member)
        {
            // 빈공간도 comment로 치기로 한다.
            if ( member.IsEmpty() )
                return true;

            return member[0] == '#';
        }

        static DataTableFieldTypeNode CreateNode(int row, FieldInfo field)
        {
            FieldTypeId id = ReadTypeId( field.FieldType );
            return new DataTableFieldTypeNode( row, field, id );
        }
    }
}
