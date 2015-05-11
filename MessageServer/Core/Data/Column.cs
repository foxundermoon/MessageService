using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;
using System.Data;

namespace MessageService.Core.Data
{
   public class Column : DataColumn
    {
        public string DbType { get; set; }
        public int Length { get; set; }
        public Column() : base(){
        }
        public Column(string name):base(name)
        { }
        public Column(string name, string dbType)
            : base(name)
        {
            DbType = dbType;
        }

        public Column(DataColumn c)
        {
            // TODO: Complete member initialization
            ColumnName = c.ColumnName;
            DataType = c.DataType;
        }
    }
}
