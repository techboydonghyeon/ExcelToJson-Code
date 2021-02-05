using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace DHLib
{
    public class GameDataJsonHandler
    {
        string mRelativePath;

        public GameDataJsonHandler(string relativePath)
        {
            mRelativePath = relativePath;
        }

        public void Serialize(IEnumerable<DataTable> tables)
        {
            if ( !Directory.Exists( mRelativePath ) )
                Directory.CreateDirectory( mRelativePath );

            foreach ( DataTable table in tables )
            {
                string path = Path.Combine(mRelativePath, table.Name + ".json");
                using ( var stream = File.Create(path) )
                using ( var writer = new StreamWriter(stream) )
                {
                    var record = table.ToRecord();
                    var json = JsonConvert.SerializeObject( record );
                    writer.Write( json );
                }
            }
        }

        public IEnumerable<DataTable> Deserialize(IEnumerable<string> paths)
        {
            foreach ( string path in paths )
            {
                using ( var stream = File.OpenRead( path ) )
                using ( var reader = new StreamReader(stream) )
                {
                    var record = JsonConvert.DeserializeObject<DataTableRecord>( reader.ReadToEnd() );
                    yield return new DataTable( record );
                }
            }
        }
    }
}