using System;

namespace DHLib
{
    public class DataTableRecord
    {
        public static readonly DataTableRecord Empty = new DataTableRecord();

        public string Name;
        public string[,] Data;
        public int RowPivot;
        public int ColPivot;
        public bool IsTransposed;
    }
}