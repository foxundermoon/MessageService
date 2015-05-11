using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data.Common;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
namespace MessageService.Core.Utils {
    public class MysqlHelper {
        static private string getMysqlConnectionStr( ) {
            var sb = new StringBuilder();
            sb.Append("Server=").Append(ConfigurationManager.AppSettings["MysqlServer"]).Append("; ")
                .Append("Database=").Append(ConfigurationManager.AppSettings["MysqlDatabase"]).Append("; ")
                .Append("User=").Append(ConfigurationManager.AppSettings["MysqlUser"]).Append("; ")
                .Append("Password=").Append(ConfigurationManager.AppSettings["MysqlPassword"]).Append("; ")
                .Append("Charset=").Append(ConfigurationManager.AppSettings["MysqlConnectionCharset"]).Append("; ")
                .Append("Pooling=").Append(ConfigurationManager.AppSettings["MysqlPooling"]).Append("; ")
                .Append("Max Pool Size=").Append(ConfigurationManager.AppSettings["MysqlMaxPoolSize"]).Append("; ")
                .Append("Use Procedure Bodies=").Append(ConfigurationManager.AppSettings["MysqlUseProcedureBodies"]).Append("; ")
                .Append("Allow Zero Datetime=").Append("true").Append("; ");
            return sb.ToString();
        }

        //数据库连接字符串(web.config来配置)，可以动态更改connectionString支持多数据库. 
        // public static string connectionString = ConfigurationManager.ConnectionStrings["ConnDB"].ConnectionString; 
        public static string connectionString =getMysqlConnectionStr();
        //public string m = ConfigurationManager.AppSettings["MySQL"]; 
        public MysqlHelper( ) { }
        #region ExecuteNonQuery
        //执行SQL语句，返回影响的记录数 
        /// <summary> 
        /// 执行SQL语句，返回影响的记录数 
        /// </summary> 
        /// <param name="SQLString">SQL语句</param> 
        /// <returns>影响的记录数</returns> 
        public static int ExecuteNonQuery( string SQLString ) {
            using(MySqlConnection connection = new MySqlConnection(connectionString)) {
                using(MySqlCommand cmd = new MySqlCommand(SQLString, connection)) {
                    try {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    } catch(MySql.Data.MySqlClient.MySqlException e) {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }
        /// <summary> 
        /// 执行SQL语句，返回影响的记录数 
        /// </summary> 
        /// <param name="SQLString">SQL语句</param> 
        /// <returns>影响的记录数</returns> 
        public static int ExecuteNonQuery(string SQLString, params MySqlParameter[] cmdParms)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e)
                    {
                        throw e;
                    }
                }
            }
        }
        /// <summary> 
        /// 执行SQL语句，返回影响的记录数 
        /// </summary> 
        /// <param name="SQLString">SQL语句</param> 
        /// <returns>影响的记录数</returns> 
        //public static int ExecuteNonQuery(string SQLString, MySqlParameter[] cmdParms)
        //{
        //    using (MySqlConnection connection = new MySqlConnection(connectionString))
        //    {
        //        using (MySqlCommand cmd = new MySqlCommand())
        //        {
        //            try
        //            {
        //                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
        //                int rows = cmd.ExecuteNonQuery();
        //                cmd.Parameters.Clear();
        //                return rows;
        //            }
        //            catch (MySql.Data.MySqlClient.MySqlException e)
        //            {
        //                throw e;
        //            }
        //        }
        //    }
        //}
        //执行多条SQL语句，实现数据库事务。 
        /// <summary> 
        /// 执行多条SQL语句，实现数据库事务。 
        /// </summary> 
        /// <param name="SQLStringList">多条SQL语句</param> 
        public static bool ExecuteNoQueryTran( List<String> SQLStringList ) {
            using(MySqlConnection conn = new MySqlConnection(connectionString)) {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                MySqlTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try {
                    for(int n = 0; n < SQLStringList.Count; n++) {
                        string strsql = SQLStringList[n];
                        if(strsql.Trim().Length > 1) {
                            cmd.CommandText = strsql;
                            PrepareCommand(cmd, conn, tx, strsql, null);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                    return true;
                } catch {
                    tx.Rollback();
                    return false;
                }
            }
        }
        #endregion
        #region ExecuteScalar
        /// <summary> 
        /// 执行一条计算查询结果语句，返回查询结果（object）。 
        /// </summary> 
        /// <param name="SQLString">计算查询结果语句</param> 
        /// <returns>查询结果（object）</returns> 
        public static object ExecuteScalar( string SQLString ) {
            using(MySqlConnection connection = new MySqlConnection(connectionString)) {
                using(MySqlCommand cmd = new MySqlCommand(SQLString, connection)) {
                    try {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value))) {
                            return null;
                        } else {
                            return obj;
                        }
                    } catch(MySql.Data.MySqlClient.MySqlException e) {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }
        /// <summary> 
        /// 执行一条计算查询结果语句，返回查询结果（object）。 
        /// </summary> 
        /// <param name="SQLString">计算查询结果语句</param> 
        /// <returns>查询结果（object）</returns> 
        public static object ExecuteScalar( string SQLString, params MySqlParameter[] cmdParms ) {
            using(MySqlConnection connection = new MySqlConnection(connectionString)) {
                using(MySqlCommand cmd = new MySqlCommand()) {
                    try {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value))) {
                            return null;
                        } else {
                            return obj;
                        }
                    } catch(MySql.Data.MySqlClient.MySqlException e) {
                        throw e;
                    }
                }
            }
        }
        #endregion
        #region ExecuteReader
        /// <summary> 
        /// 执行查询语句，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close ) 
        /// </summary> 
        /// <param name="strSQL">查询语句</param> 
        /// <returns>MySqlDataReader</returns> 
        public static MySqlDataReader ExecuteReader( string strSQL ) {
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand(strSQL, connection);
            try {
                connection.Open();
                MySqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return myReader;
            } catch(MySql.Data.MySqlClient.MySqlException e) {
                throw e;
            }
        }
        /// <summary> 
        /// 执行查询语句，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close ) 
        /// </summary> 
        /// <param name="strSQL">查询语句</param> 
        /// <returns>MySqlDataReader</returns> 
        public static MySqlDataReader ExecuteReader( string SQLString, params MySqlParameter[] cmdParms ) {
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand();
            try {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                MySqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return myReader;
            } catch(MySql.Data.MySqlClient.MySqlException e) {
                throw e;
            }
            // finally 
            // { 
            // cmd.Dispose(); 
            // connection.Close(); 
            // } 
        }
        #endregion
        #region ExecuteDataTable
        /// <summary> 
        /// 执行查询语句，返回DataTable 
        /// </summary> 
        /// <param name="SQLString">查询语句</param> 
        /// <returns>DataTable</returns> 
        public static DataTable ExecuteDataTable( string SQLString ) {
            using(MySqlConnection connection = new MySqlConnection(connectionString)) {
                DataSet ds = new DataSet();
                try {
                    connection.Open();
                    MySqlDataAdapter command = new MySqlDataAdapter(SQLString, connection);
                    command.Fill(ds, "ds");
                } catch(MySql.Data.MySqlClient.MySqlException ex) {
                    throw new Exception(ex.Message);
                }
                catch (Exception other)
                {
                    throw new Exception(other.Message);
                }
                return ds.Tables[0];
            }
        }
        /// <summary>
        /// 判断表里是否有符合该sql语句的数据
        /// </summary>
        /// <param name="SQLString"></param>
        /// <returns></returns>
        public static bool ExecuteQueryHasRows( string SQLString ) {
            var flag = false;
            try {
                var reader = ExecuteReader(SQLString);
                flag =  reader.HasRows;
                reader.Close();
            } catch(MySqlException e) {
                flag = false;
            }
            return flag;
        }
        /// <summary> 
        /// 执行查询语句，返回DataSet 
        /// </summary> 
        /// <param name="SQLString">查询语句</param> 
        /// <returns>DataTable</returns> 
        public static DataTable ExecuteDataTable( string SQLString, params MySqlParameter[] cmdParms ) {
            using(MySqlConnection connection = new MySqlConnection(connectionString)) {
                MySqlCommand cmd = new MySqlCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using(MySqlDataAdapter da = new MySqlDataAdapter(cmd)) {
                    DataSet ds = new DataSet();
                    try {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    } catch(MySql.Data.MySqlClient.MySqlException ex) {
                        throw new Exception(ex.Message);
                    }
                    return ds.Tables[0];
                }
            }
        }
        //获取起始页码和结束页码 
        public static DataTable ExecuteDataTable( string cmdText, int startResord, int maxRecord ) {
            using(MySqlConnection connection = new MySqlConnection(connectionString)) {
                DataSet ds = new DataSet();
                try {
                    connection.Open();
                    MySqlDataAdapter command = new MySqlDataAdapter(cmdText, connection);
                    command.Fill(ds, startResord, maxRecord, "ds");
                } catch(MySql.Data.MySqlClient.MySqlException ex) {
                    throw new Exception(ex.Message);
                }
                return ds.Tables[0];
            }
        }
        #endregion

        #region 创建command
        private static void PrepareCommand( MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, string cmdText, MySqlParameter[] cmdParms ) {
            if(conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if(trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType; 
            if(cmdParms != null) {
                foreach(MySqlParameter parameter in cmdParms) {
                    if((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) && 
(parameter.Value == null)) {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }
        #endregion
    }
}