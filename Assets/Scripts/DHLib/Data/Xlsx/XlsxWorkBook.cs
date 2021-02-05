using System.Collections.Generic;
using System.Xml;
using SharpCompress.Archive.Zip;

namespace DHLib
{
    public class XlsxWorkBook
    {
        List<string> mSST;
        Dictionary<string, XlsxWorkSheet> mSheets;

        public IList<string> SST => mSST;
        public IEnumerable<XlsxWorkSheet> Sheets => mSheets.Values;

        public XlsxWorkBook(ZipArchiveEntry workbook, ZipArchiveEntry workbookRels, ZipArchiveEntry sharedStrings)
        {
            ReadWorkbook(workbook);
            ReadWorkbookRels(workbookRels);
            ReadSharedStrings(sharedStrings);
        }

        void ReadWorkbook(ZipArchiveEntry workbook)
        {
            mSheets = new Dictionary<string, XlsxWorkSheet>();

            using ( var stream = workbook.OpenEntryStream() )
            using ( var reader = XmlReader.Create( stream ) )
            {
                reader.ReadToFollowing( "sheets" );
                while ( reader.Read() )
                {
                    if ( reader.GetLocalElement( "sheet" ) )
                    {
                        var rId = reader.GetAttribute( "r:id" );
                        var worksheet = new XlsxWorkSheet();

                        mSheets.Add( rId, worksheet );
                    }
                }
            }
        }

        void ReadWorkbookRels(ZipArchiveEntry workbookRels)
        {
            using ( var stream = workbookRels.OpenEntryStream() )
            using ( var reader = XmlReader.Create( stream ) )
            {
                while ( reader.Read() )
                {
                    if ( reader.GetLocalElement( "Relationship" ) )
                    {
                        var rId = reader.GetAttribute( "Id" );
                        if ( mSheets.ContainsKey( rId ) )
                        {
                            var target = reader.GetAttribute( "Target" );
                            // Sanitize string.
                            if ( target.StartsWith( "/xl/" ) )
                                target = target.Substring( 1 );
                            else
                                target = "xl/" + target;

                            mSheets[rId].Path = target;
                        }
                    }
                }
            }
        }

        void ReadSharedStrings(ZipArchiveEntry sharedStrings)
        {
            mSST = new List<string>();

            using ( var stream = sharedStrings.OpenEntryStream() )
            using ( var reader = XmlReader.Create( stream ) )
            {
                // phonetic은 무시한다.
                while ( reader.Read() )
                {
                    if ( reader.GetLocalElement( "t" ) )
                    {
                        string va = reader.ReadElementContentAsString();
                        if (va == "[EffectData]*" )
                        {

                        }
                        mSST.Add(va);
                    }
                }
            }
        }
    }
}