using DHLib;
using System.IO;
using UnityEngine;

namespace BTDungeon
{
    public class BTDungeon
    {
        public static GameData GameData;

        public void Initialize()
        {
            var path = Path.Combine( Application.dataPath, "ResourceMain/Datasheet" );
            var reader = new ResourceFolderReader( path );
            var items = reader.GetFullPaths( "*.xlsx" );
            if ( items.Length == 0 )
                return;

            Datasheet.ReadFromXlsx( GameData, items );
        }
    }
}