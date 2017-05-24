using System;
using System.Collections.Generic;
using System.Net.Configuration;

namespace AiqlWrapper
{
    internal class TablesWrapper
    {
        public ResultTable[] Tables { get; set; }
    }
    public class ResultTable
    {
        public string TableName { get; set; }
        public Column[] Columns { get; set; }
        public object[][] Rows { get; set; }
    }

    public class Column
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public string ColumnType { get; set; }
    }
}
