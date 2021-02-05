using System.Collections.Generic;
using System.Xml;

namespace DHLib
{
    public class XlsxToDataTableConverter
    {
        List<DataTable> mDataTables;

        public IEnumerable<DataTable> Tables => mDataTables;

        public XlsxToDataTableConverter(string path)
        {
            var reader = new XlsxXmlReader( path );
            ReadDataTables( reader );
        }

        void ReadDataTables(XlsxXmlReader xlsxReader)
        {
            var records = new List<DataTableRecord>();
            mDataTables = new List<DataTable>();

            foreach ( var sheet in xlsxReader.Workbook.Sheets )
            {
                var data = new string[sheet.Dimension.RowCount, sheet.Dimension.ColCount];

                using ( var stream = xlsxReader.Open( sheet.Path ) )
                using ( var reader = XmlReader.Create( stream ) )
                {
                    reader.ReadToFollowing( "sheetData" );
                    // elements.
                    string r = string.Empty;
                    string s = string.Empty;
                    string t = string.Empty;
                    int row = 0;
                    int col = 0;
                    bool isNextValue = false;

                    while ( reader.Read() )
                    {
                        if ( reader.GetLocalElement( "c" ) )
                        {
                            r = reader.GetAttribute( "r" );
                            s = reader.GetAttribute( "s" );
                            t = reader.GetAttribute( "t" );
                            (row, col) = XlsxDimension.ParseReference( r );
                            isNextValue = false;
                            continue;
                        }

                        if ( reader.LocalName == "v" || reader.LocalName == "t" )
                        {
                            isNextValue = true;
                            continue;
                        }

                        if ( reader.NodeType == XmlNodeType.Text && isNextValue )
                        {
                            string value = reader.Value;

                            if ( t == "s" )
                            {
                                value = xlsxReader.Workbook.SST[int.Parse( value )];

                                var record = ParseDataTableRecord( value );
                                if ( record.Name.IsValid() )
                                {
                                    record.Data = data;
                                    record.RowPivot = row;
                                    record.ColPivot = col - 1;

                                    records.Add( record );
                                }
                            }

                            // 엑셀은 1,1 부터 인덱스가 시작한다.
                            data[row - 1, col - 1] = value;
                        }
                    }
                }
            }

            foreach ( var record in records )
                mDataTables.Add( new DataTable( record ) );
        }

        DataTableRecord ParseDataTableRecord(string value)
        {
            if ( value.Length > 5 )
            {
                if ( value[0] == '[' && value.Contains( "Data" ) )
                {
                    int last = value.Length - 1;
                    bool isTransposed = false;

                    if ( value[last] == '*' )
                    {
                        isTransposed = true;
                        last--;
                    }

                    if ( value[last] == ']' )
                    {
                        var record = new DataTableRecord();
                        record.Name = value.Substring( 1, last - 1 );
                        record.IsTransposed = isTransposed;
                        return record;
                    }
                }
            }

            return DataTableRecord.Empty;
        }
    }
}