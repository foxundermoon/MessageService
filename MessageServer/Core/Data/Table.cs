using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace MessageService.Core.Data
{
    public class Table :DataTable
    {
        public ColumnCollection DataColumns { get; set; }
        public string Database { get; set; }
        public Table() : base() {
            DataColumns = new ColumnCollection(this);
        }
        public Table(params Column[] columns):base(){
            DataColumns = new ColumnCollection(this);
            this.DataColumns.AddRange(columns);
        }
        public Table(params string[] columnName)
            : base()
        {
            DataColumns = new ColumnCollection(this);
            foreach (var name in columnName)
            {
                this.DataColumns.Add(name);
            }
        }

        public Table(DataTable dt)
        {
            // TODO: Complete member initialization
            DataColumns = new ColumnCollection(this);
            foreach(DataColumn c in dt.Columns){
                DataColumns.Add(c);
            }
            foreach (DataRow r in dt.Rows)
            {
                Rows.Add(r.ItemArray);
            }
        }
    }
}
