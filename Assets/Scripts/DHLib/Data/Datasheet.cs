using BTDungeon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DHLib
{
    public static class Datasheet
    {
        public static DataTableParseOptions Options = new DataTableParseOptions();

        static Datasheet() =>
            DatasheetTemplate.Initialize();

        public static void WriteCode(string filePath, string genFilePath, string @namespace)
        {
            var parser = new XlsxToDataTableConverter( filePath );
            var codeGen = new GameDataCodeGenerator( genFilePath );

            codeGen.GenerateCode( @namespace, parser.Tables );
        }

        public static void WriteCode(IEnumerable<string> filePaths, string genFilePath, string jsonPath, string @namespace)
        {
            var tables = new List<DataTable>();
            foreach ( string path in filePaths )
            {
                var parser = new XlsxToDataTableConverter( path );
                tables.AddRange( parser.Tables );
            }            

            var codeGen = new GameDataCodeGenerator( genFilePath );
            codeGen.GenerateCode( @namespace, tables );

            var jsonGenerator = new GameDataJsonHandler( jsonPath );
            jsonGenerator.Serialize( tables );
        }

        public static void ReadFromXlsx<T>(T target, string path)
        {
            var parser = new XlsxToDataTableConverter( path );
            DeserializeInternal( target, parser.Tables.ToList() );
        }

        public static void ReadFromXlsx<T>(T target, IEnumerable<string> pathList)
        {
            var tables = new List<DataTable>();

            foreach ( string path in pathList )
            {
                var parser = new XlsxToDataTableConverter( path );
                tables.AddRange( parser.Tables );
            }

            DeserializeInternal( target, tables );
        }

        public static void ReadFromXlsx2<T>(T target, IEnumerable<string> pathList) where T : IGameData
        {
            var tables = new List<DataTable>();

            foreach ( string path in pathList )
            {
                var parser = new XlsxToDataTableConverter( path );
                tables.AddRange( parser.Tables );
            }

            target.Read( tables );
        }

        public static void ReadFromXlsx3<T>(T target, IEnumerable<string> pathList) where T : IGameData
        {
            var json = new GameDataJsonHandler( null );
            target.Read( json.Deserialize( pathList ) );
        }

        public static void ReadFromXlsx4<T>(T target, IEnumerable<string> pathList) where T : IGameData
        {
            var json = new GameDataJsonHandler( null );
            DeserializeInternal( target, json.Deserialize( pathList ) );
        }

        static void DeserializeInternal<T>(T target, IEnumerable<DataTable> tables)
        {
            var lookupTable = new Dictionary<string, DataTable>();

            foreach ( DataTable table in tables )
                lookupTable.Add( table.Name, table );

            var targetFlags = BindingFlags.Public | BindingFlags.Instance;
            var fields = typeof(T).GetFields( targetFlags );

            foreach ( FieldInfo field in fields )
            {
                if ( field.HasCustomAttribute<NonSerializedAttribute>() )
                    continue;

                if ( lookupTable.TryGetValue( field.Name, out var table ) )
                {
                    var value = DataTableFieldTypeReader.Read( field.FieldType, table );
                    field.SetValue( target, value );
                }
            }
        }

        public static void SetCustomParser<T>(DataTableParseOptions.ParseValue customParser)
        {
            Options.SetCustomParser<T>( customParser );
        }
    }
}