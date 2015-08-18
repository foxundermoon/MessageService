using FoxundermoonLib.Database.Mysql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.App_Start.Tools
{
    public class AlterTables
    {
        public static void AddOneCoulmn()
        {

            var sql = MysqlHelper.connectionString;
            var tables = MysqlHelper.ExecuteDataTable("use foxdata ; show tables;");
            foreach (DataRow r in tables.Rows)
            {
                string name = r[0] as string;
                string alterSql = string.Format("alter table {0} add column LID int;", name);
                if (name.StartsWith("nj_专题"))
                {
                    MysqlHelper.ExecuteNonQuery(alterSql);
                }

            }
        }
        public static void Change版本列()
        {
            var sql = MysqlHelper.connectionString;
            var tables = MysqlHelper.ExecuteDataTable("use yzrq ; show tables;");
            foreach (DataRow r in tables.Rows)
            {
                string name = r[0] as string;
                var fillSql = string.Format("UPDATE `{0}` SET  `版本`='1' WHERE `版本` is NULL;", name);
                string alterSql = string.Format("ALTER TABLE `{0}` MODIFY COLUMN `版本`  int(10) NOT NULL DEFAULT 1", name);
                {
                    try
                    {

                        MysqlHelper.ExecuteNonQuery(fillSql);
                        MysqlHelper.ExecuteNonQuery(alterSql);
                    }
                    catch (Exception ignore) { }

                }

            }
        }
        public static void Change是否历史()
        {
            var sql = MysqlHelper.connectionString;
            var tables = MysqlHelper.ExecuteDataTable("use yzrq ; show tables;");
            foreach (DataRow r in tables.Rows)
            {
                string name = r[0] as string;
                var fillSql = string.Format("UPDATE `{0}` SET  `是否历史`='否' WHERE `是否历史` is NULL;", name);
                string alterSql = string.Format("ALTER TABLE `{0}` MODIFY COLUMN `是否历史`  varchar(5) NOT NULL DEFAULT  否", name);
                {
                    try
                    {

                        MysqlHelper.ExecuteNonQuery(fillSql);
                        MysqlHelper.ExecuteNonQuery(alterSql);
                    }
                    catch (Exception ignore) { }

                }

            }
        }


    }
}
