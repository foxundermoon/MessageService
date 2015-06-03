using FoxundermoonLib.Database.Mysql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.App_Start.Tools
{
  public  class AlterTables
    {
      public static void AddOneCoulmn(){

          var sql = MysqlHelper.connectionString;
            var tables = MysqlHelper.ExecuteDataTable("use foxdata ; show tables;");
            foreach (DataRow r in tables.Rows)
            {
                string name = r[0] as string;
                string alterSql=string.Format("alter table {0} add column LID int;",name);
                if (name.StartsWith("nj_专题"))
                {
                    MysqlHelper.ExecuteNonQuery(alterSql);
                }

            }

      }
        

    }
}
