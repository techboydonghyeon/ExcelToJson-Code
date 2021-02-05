using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Newtonsoft;
using Newtonsoft.Json;

namespace DHLib
{
    public class DataTable
    {
        string mName;
        string[,] mData;
        int mRowPivot;
        int mColPivot;
        int mRowCount;
        int mColCount;
        bool mIsTransposed;

        public string Name => mName;
        public int MemberCount => mRowCount;
        public int ValueCount => mColCount - 2;

        internal DataTable(DataTableRecord record)
        {
            mName = record.Name;
            mData = record.Data;
            mRowPivot = record.RowPivot;
            mColPivot = record.ColPivot;
            mIsTransposed = record.IsTransposed;
            SetDataTableSize();
        }

        public DataTableRecord ToRecord()
        {
            var data = new string[mRowCount, mColCount];
            for (int i = 0; i < mRowCount; ++i )
            {
                for (int j = 0; j < mColCount; ++j )
                {
                    data[i, j] = ReadDataInternal( i, j );
                }
            }

            return new DataTableRecord()
            {
                Name = mName,
                Data = data,
                RowPivot = 0,
                ColPivot = 0,
                IsTransposed = false,
            };
        }

        public string GetType(int row)
        {
            return ReadDataInternal( row, 0 );
        }

        public string GetMember(int row)
        {
            return ReadDataInternal( row, 1 );
        }

        public string GetValue(int row, int col)
        {
            return ReadDataInternal( row, col + 2 );
        }

        void SetDataTableSize()
        {
            int rowMax = mData.GetLength(0);
            int colMax = mData.GetLength(1);

            // 기본값으로 초기화된다.
            if ( mRowPivot >= rowMax || mColPivot >= colMax )
                return;

            // Column
            int colStart = mColPivot;
            int colLast = colStart;
            for ( ; colLast < colMax; ++colLast )
            {
                string value = mData[mRowPivot, colLast];
                if ( value.IsEmpty() )
                    break;
            }

            // Row
            int rowStart = mRowPivot;
            int rowLast = rowStart;
            for ( ; rowLast < rowMax; ++rowLast )
            {
                bool isEmpty = true;
                for ( int col = mColPivot; col < colLast; ++col )
                {
                    string value = mData[rowLast, col];
                    if ( value.IsValid() )
                    {
                        isEmpty = false;
                        break;
                    }
                }
                if ( isEmpty )
                    break;
            }

            if ( mIsTransposed )
            {
                mColCount = rowLast - rowStart;
                mRowCount = colLast - colStart;
            }
            else
            {
                mColCount = colLast - colStart;
                mRowCount = rowLast - rowStart;
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        string ReadDataInternal(int row, int col)
        {
            if ( mIsTransposed )
            {
                int temp = row;
                row = col;
                col = temp;
            }

            int rows = mRowPivot + row;
            int cols = mColPivot + col;

            return mData[rows, cols];
        }
    }
}
