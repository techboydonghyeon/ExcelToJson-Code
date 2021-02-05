
using System.Collections.Generic;
using System.IO;
using System.Xml;
using SharpCompress.Archive.Zip;

namespace DHLib
{
    public class XlsxXmlReader
    {
        ZipArchive mArchive;
        Dictionary<string, ZipArchiveEntry> mEntries;
        XlsxWorkBook mWorkbook;

        public XlsxWorkBook Workbook => mWorkbook;

        public XlsxXmlReader(string path)
        {
            using( var stream = File.Open(path, FileMode.Open) )
            {
                mArchive = ZipArchive.Open(stream);
                mEntries = new Dictionary<string, ZipArchiveEntry>();

                foreach ( var entry in mArchive.Entries )
                    mEntries.Add(entry.FilePath, entry);

                mWorkbook = new XlsxWorkBook(
                    mEntries["xl/workbook.xml"],
                    mEntries["xl/_rels/workbook.xml.rels"],
                    mEntries["xl/sharedStrings.xml"]
                    );

                foreach ( var sheet in mWorkbook.Sheets )
                    ReadWorkSheet(sheet);
            }
        }

        public Stream Open(string path)
        {
            return mEntries[path].OpenEntryStream();
        }

        void ReadWorkSheet(XlsxWorkSheet sheet)
        {
            using ( var stream = mEntries[sheet.Path].OpenEntryStream() )
            using ( var reader = XmlReader.Create( stream ) )
            {
                while ( reader.Read() )
                {
                    if ( reader.GetLocalElement( "dimension" ) )
                    {
                        var dimension = reader.GetAttribute("ref");
                        sheet.Dimension = new XlsxDimension( dimension );
                        break;
                    }
                }
            }
        }
    }
}