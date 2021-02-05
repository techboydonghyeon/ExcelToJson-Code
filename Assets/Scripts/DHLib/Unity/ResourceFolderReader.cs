using System.IO;
using System.Linq;

namespace DHLib
{
    class ResourceFolderReader
    {
        string mBaseFolder;

        public ResourceFolderReader(string baseFolder)
        {
            mBaseFolder = baseFolder;
        }

        public string[] SearchFiles(string pattern)
        {
            return Directory.GetFiles( mBaseFolder, pattern, SearchOption.AllDirectories )
                .Select( x => x.StartsWith( mBaseFolder ) ? x.Substring( mBaseFolder.Length ) : x )
                .Where( x => Path.GetFileName( x )[0] != '~' )
                .ToArray();
        }

        public string[] GetFullPaths(string pattern)
        {
            return Directory.GetFiles( mBaseFolder, pattern, SearchOption.AllDirectories )
                .Where( x => Path.GetFileName( x )[0] != '~' )
                .ToArray();
        }
    }
}
