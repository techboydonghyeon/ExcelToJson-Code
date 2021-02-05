using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DHLib
{
    public class GameDataCodeGenerator : CodeGenerator
    {
        class EnumSegment
        {
            public string Name;
            public HashSet<string> Constants = new HashSet<string>();
        }

        class DictionarySegment
        {
            public string KeyName;
            public string KeyType;
        }

        string mFilePath;

        Dictionary<string, EnumSegment> mEnums;
        Dictionary<string, DictionarySegment> mDictionaries;

        public GameDataCodeGenerator(string filePath)
        {
            mFilePath = filePath;

            mEnums = new Dictionary<string, EnumSegment>();
            mDictionaries = new Dictionary<string, DictionarySegment>();
        }

        public void GenerateCode(string @namespace, IEnumerable<DataTable> tables)
        {
            PopulateCodeSegments( tables );
            Header( nameof(GameDataCodeGenerator) );
            using ( Namespace( @namespace ) )
            {
                CreateGameData( tables );
                NewLine( 2 );
                Comment( "----------------------------------------------" );
                Comment( "Class" );
                Comment( "----------------------------------------------" );
                NewLine( 1 );
                CreateClass( tables );
                NewLine( 1 );
                Comment( "----------------------------------------------" );
                Comment( "Enums" );
                Comment( "----------------------------------------------" );
                NewLine( 1 );
                CreateEnums();
            }
            CreateFile( mFilePath );
        }

        void CreateGameData(IEnumerable<DataTable> tables)
        {
            using ( Class( AccessMode.Public, "GameData", "IGameData" ) )
            {
                NewLine( 1 );
                foreach ( DataTable table in tables )
                {
                    if ( table.ValueCount == 1 )
                    {
                        Member( AccessMode.Public, table.Name, table.Name );
                    }
                    else if ( mDictionaries.TryGetValue( table.Name, out var dicSegment ) )
                    {
                        MemberDictionary( AccessMode.Public, dicSegment.KeyType, table.Name, table.Name );
                    }
                    else
                    {
                        MemberList( AccessMode.Public, table.Name, table.Name );
                    }
                }

                NewLine( 1 );
                Comment( "----------------------------------------------" );
                Comment( "Read Methods." );
                Comment( "----------------------------------------------" );
                WriteLine( "#region Read" );

                using ( Method( AccessMode.Public, "Read", null, "IEnumerable<DataTable> tables" ) )
                {
                    WriteLine( "var tableParser = new Dictionary<string, Action<DataTable>>();" );
                    foreach ( DataTable table in tables )
                    {
                        WriteLine( $"tableParser.Add(\"{table.Name}\", Parse{table.Name});" );
                    }

                    NewLine( 1 );
                    using ( Brace( "foreach( var table in tables )" ) )
                    {
                        WriteLine( $"tableParser[table.Name]( table );" );
                    }
                }

                foreach ( DataTable table in tables )
                {
                    NewLine( 1 );
                    using ( Method( AccessMode.Private, $"Parse{table.Name}", null, "DataTable table" ) )
                    {
                        WriteTableParseMethod( table );
                    }
                }

                WriteParseMethod();
                WriteLine( "#endregion" );
            }
        }

        void WriteTableParseMethod(DataTable table)
        {
            if ( table.ValueCount == 1 )
            {
                WriteLine( $"{table.Name} = new {table.Name}();" );
                // WriteMemberValues를 공유하기위해 추가.
                using ( WriteMemberValues( table, $"var data = {table.Name};" ) ) { }
            }
            else if ( mDictionaries.TryGetValue( table.Name, out var dicSegment ) )
            {
                WriteLine( $"{table.Name} = new Dictionary<{dicSegment.KeyType}, {table.Name}>();" );
                using ( WriteMemberValues( table, $"var data = new {table.Name}();" ) )
                {
                    WriteLine( $"{table.Name}.Add( data.{dicSegment.KeyName}, data );" );
                }
            }
            else
            {
                WriteLine( $"{table.Name} = new List<{table.Name}>();" );
                using ( WriteMemberValues( table, $"var data = new {table.Name}();" ) )
                {
                    WriteLine( $"{table.Name}.Add( data );" );
                }
            }
        }

        Scope WriteMemberValues(DataTable table, string @new)
        {
            Scope scope = new Scope();
            if ( table.MemberCount > 1 )
            {
                scope = Brace( "for ( int i = 0; i < table.ValueCount; ++i )" );
            }
            else
            {
                WriteLine( "int i = 0;" );
            }

            WriteLine( @new );
            for ( int i = 0; i < table.MemberCount; ++i )
            {
                string type = table.GetType( i );
                if ( type.StartsWith( "enum." ) )
                    type = type.Substring( 5 );

                string member = table.GetMember( i );
                if ( member.IsEmpty() || member[0] == '#' )
                    continue;

                if ( type.EndsWith( "[]" ) )
                {
                    var realType = type.Substring( 0, type.Length - 2 );
                    type = realType + "Array";
                    int arrayLength = 0;
                    for ( int j = i + 1; j < table.MemberCount; ++j )
                    {
                        string element = table.GetMember( j );
                        if ( element.IsEmpty() || element[0] == '#' )
                        {
                            arrayLength++;
                            continue;
                        }
                    }

                    WriteLine( $"data.{member} = new {realType}[{arrayLength}];" );
                    using ( Brace( $"for ( int x{i} = 0; x{i} < {arrayLength}; ++x{i} )" ) )
                    {
                        WriteLine( $"data.{member}[x{i}] = Parse{realType}(table, {i} + x{i}, i);" );
                    }
                    continue;
                }

                WriteLine( $"data.{member} = Parse{type}(table, {i}, i);" );
            }
            return scope;
        }

        Scope CreateParseMethod(string @return)
        {
            return Method( AccessMode.Private, $"Parse{@return}", @return, "DataTable table,", "int row,", "int col" );
        }

        void CreateCustomType(string name)
        {
            WriteLine( $"object value = DatasheetTemplate.Parse{name}(table.GetValue(row, col));" );
            WriteLine( "if ( value == null )" );
            WriteLine( $"\treturn new {name}();" );
            WriteLine( $"return ({name})value;" );
        }

        void WriteParseMethod()
        {
            using ( CreateParseMethod( "int" ) )
            {
                WriteLine( "return int.Parse( table.GetValue(row, col) );" );
            }

            using ( CreateParseMethod( "float" ) )
            {
                WriteLine( "return float.Parse( table.GetValue(row, col) );" );
            }

            using ( CreateParseMethod( "long" ) )
            {
                WriteLine( "return long.Parse( table.GetValue(row, col) );" );
            }

            using ( CreateParseMethod( "string" ) )
            {
                WriteLine( "return table.GetValue(row, col);" );
            }

            using ( CreateParseMethod( "CSVInt" ) )
            {
                CreateCustomType( "CSVInt" );
            }

            using ( CreateParseMethod( "CSVFloat" ) )
            {
                CreateCustomType( "CSVFloat" );
            }

            using ( CreateParseMethod( "CSVString" ) )
            {
                CreateCustomType( "CSVString" );
            }

            using ( CreateParseMethod( "RangeInt" ) )
            {
                CreateCustomType( "RangeInt" );
            }

            using ( CreateParseMethod( "RangeFloat" ) )
            {
                CreateCustomType( "RangeFloat" );
            }

            using ( CreateParseMethod( "bool" ) )
            {
                WriteLine( "var value = table.GetValue(row, col);" );
                WriteLine( "if (value.IsEmpty())" );
                WriteLine( "\treturn false;" );
                WriteLine( "return bool.Parse(value);" );
            }

            foreach ( EnumSegment e in mEnums.Values )
            {
                using ( CreateParseMethod( e.Name ) )
                {
                    WriteLine( $"return ({e.Name})Enum.Parse(typeof({e.Name}), table.GetValue(row, col));" );
                }
            }
        }

        void CreateClass(IEnumerable<DataTable> tables)
        {
            foreach ( DataTable table in tables )
            {
                using ( Class(AccessMode.Public, table.Name) )
                {
                    for ( int i = 0; i < table.MemberCount; ++i )
                    {
                        string type = table.GetType( i );
                        if ( type.StartsWith( "enum." ) )
                            type = type.Substring( 5 );

                        string member = table.GetMember( i );
                        if ( member.IsEmpty() || member[0] == '#' )
                            continue;

                        Member( AccessMode.Public, type, member );
                    }
                }
                NewLine( 1 );
            }
        }

        void CreateEnums()
        {
            foreach ( EnumSegment e in mEnums.Values )
            {
                using ( Enum(AccessMode.Public, e.Name ) )
                {
                    EnumConstant( "None" );
                    foreach ( string constant in e.Constants )
                    {
                        EnumConstant( constant );
                    }
                }
                NewLine( 1 );
            }
        }

        void PopulateCodeSegments(IEnumerable<DataTable> tables)
        {
            foreach ( DataTable table in tables )
            {
                for (int i = 0; i < table.MemberCount; ++i )
                {
                    string type = table.GetType( i );
                    if ( type.IsValid() || type[0] != '#' )
                    {
                        // NOTE: enum을 위한 fast-path. 하드코딩됨.
                        if ( type[0] == 'e' )
                        {
                            if ( type.StartsWith( "enum." ) )
                            {
                                type = type.Substring( 5 );
                                PopulateEnums( table, type, i );
                            }
                        }
                    }

                    string member = table.GetMember( i );
                    if ( !member.IsEmpty() && member[0] != '#' )
                    {
                        if ( member == "key" || member == "id" )
                            if ( !mDictionaries.ContainsKey( table.Name ) )
                                mDictionaries.Add( table.Name, new DictionarySegment() { KeyName = member, KeyType = type } );
                    }
                }
            }
        }

        void PopulateEnums(DataTable table, string name, int row)
        {
            bool alreadyHasEnum = mEnums.TryGetValue(name, out var segment);
            if ( !alreadyHasEnum )
                segment = new EnumSegment() { Name = name };

            for (int i = 0; i < table.ValueCount; ++i )
            {
                string value = table.GetValue( row, i );
                if ( value.IsEmpty() )
                    continue;

                segment.Constants.Add( value );
            }

            if ( !alreadyHasEnum )
                mEnums.Add( name, segment );
        }
    }
}