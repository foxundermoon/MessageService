using System;
//using System.Collections.Generic;
using System.Linq;
using agsXMPP.protocol.client;
using agsXMPP.protocol;
using MessageService;
using System.Diagnostics;
using MessageService.Core.Xmpp;
using agsXMPP;
using System.Data;
using System.Text;
using System.Data.Common;
using MySql.Data.MySqlClient;
using FoxundermoonLib.Database.Mysql;
using FoxundermoonLib.Encrypt;
using FoxundermoonLib.XmppEx;
using FoxundermoonLib.XmppEx.Command;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using FoxundermoonLib.Database.Sql;
namespace MessageService.Core.Xmpp
{
    public partial class XmppServer
    {
        public void OnMessage(XmppSeverConnection contextConnection, Message message)
        {
            if (contextConnection.IsAuthentic)
            {
                processMessage(contextConnection, message);
            }
            else
            {
                contextConnection.Stop();
            }
        }
        private async void processMessage(agsXMPP.XmppSeverConnection contextConnection, Message msg)
        {
            FoxundermoonLib.XmppEx.Data.Message message = new FoxundermoonLib.XmppEx.Data.Message();
            #region 转换Message
            var content = "";
            var command = "";

            if (!string.IsNullOrEmpty(msg.Language) && msg.Language.ToUpper().Contains("BASE64"))
            {
                content = EncryptUtil.DecryptBASE64ByGzip(msg.Body);
            }
            else
            {
                content = msg.Body;
                //dbmsg.Content = msg.Body;
            }
            if (msg.To != null && !string.IsNullOrEmpty(msg.To.User))
            {
                message.ToUser = new FoxundermoonLib.XmppEx.Data.User(msg.To.User, msg.To.Resource);
            }

            if (msg.From != null && msg.From.User != null)
            {
                message.FromUser = new FoxundermoonLib.XmppEx.Data.User(msg.From.User, msg.From.Resource);
            }
            command = msg.Subject;

            //转发 message
            // to != "0" and ""

            message.SetJsonMessage(content);
            message.SetJsonCommand(command);
            if (null != message.ToUser)
            {
                SmartBroadCast(message);
            }
            #endregion
            Console.WriteLine(message.ToJson(true));
            Console.WriteLine(message.GetJsonCommand());
            #region  数据表操作
            try
            {
                // default mysql datable
                databaseType dbt = getDatabaseType(message);
                if (dbt == databaseType.MySql)
                {
                    #region 有数据表操作
                    if (message.DataTable != null && message.DataTable.Rows.Count > 0)
                    {
                        var sqlb = new StringBuilder();
                        #region insert
                        if (message.Command.Operation == "insert")
                        {
                            bool hasLID = false;
                            foreach (DataColumn dc in message.DataTable.DataColumns)
                            {
                                if ("LID".Equals(dc.ColumnName))
                                    hasLID = true;
                            }
                            try
                            {
                                sqlb.Append("INSERT INTO ");
                                if (!string.IsNullOrEmpty(message.DataTable.Database))
                                {
                                    sqlb.Append(message.DataTable.Database).Append(".");
                                }
                                sqlb.Append(message.DataTable.TableName).Append("(");
                                var sbv = new StringBuilder();
                                foreach (FoxundermoonLib.XmppEx.Data.Column c in message.DataTable.DataColumns)
                                {
                                    sqlb.Append("").Append(c.ColumnName).Append(" , ");
                                    sbv.Append("@").Append(c.ColumnName).Append(",");
                                }
                                sqlb.Remove(sqlb.Length - 2, 2).Append(") VALUES (").Append(sbv.Remove(sbv.Length - 1, 1).Append(")").ToString());
                                var sql = sqlb.ToString();
                                var count = 0;
                                foreach (DataRow r in message.DataTable.Rows)
                                {
                                    int l = r.ItemArray.GetLength(0);
                                    MySqlParameter[] ps = new MySqlParameter[l];
                                    for (var i = 0; i < l; i++)
                                    {
                                        ps[i] = new MySqlParameter(message.DataTable.DataColumns[i].ColumnName, r.ItemArray[i]);
                                    }
                                    count += MysqlHelper.ExecuteNonQuery(sql, ps);
                                }
                                message.AddProperty("Count", count.ToString());
                                if (hasLID)
                                {
                                    wrapReturnTable(message,dbt);  //返回ID  LID 对应表
                                }

                            }
                            catch (Exception e)
                            {
                                message.AddProperty("error", "server error@MessageHandler DataTable insert");
                                message.AddProperty("errorMessage", e.Message);
                                Console.Write(e.Message);
                            }
                            finally
                            {
                                message.SwitchDirection();
                                message.Command.Operation = "insertResponse";
                                UniCast(contextConnection, message);
                            }
                            //INSERT INTO `nj_gps档案记录`(`ID`, `用户`, `日期`, `档案号`, `坐标串`) VALUES ([value-1],[value-2],[value-3],[value-4],[value-5])


                        }
                        #endregion
                        #region  delete
                        else if (message.Command.Operation == "delete")
                        {
                            try
                            {
                                sqlb.Append("DELETE FROM ");
                                if (!string.IsNullOrEmpty(message.DataTable.Database))
                                {
                                    sqlb.Append("`").Append(message.DataTable.Database).Append("`.");
                                }
                                sqlb.Append("`").Append(message.DataTable.TableName).Append("` WHERE ")
                                    .Append(message.Command.Condition);
                                var sql = sqlb.ToString();
                                var count = 0;
                                foreach (DataRow r in message.DataTable.Rows)
                                {
                                    int l = r.ItemArray.GetLength(0);
                                    MySqlParameter[] ps = new MySqlParameter[l];
                                    for (var i = 0; i < l; i++)
                                    {
                                        ps[i] = new MySqlParameter(message.DataTable.DataColumns[i].ColumnName, r.ItemArray[i]);
                                    }
                                    count += MysqlHelper.ExecuteNonQuery(sql, ps);

                                }
                                message.AddProperty("Count", count.ToString());
                            }
                            catch (Exception e)
                            {
                                message.AddProperty("error", "server error@MessageHandler DataTable delete");
                                message.AddProperty("errorMessage", e.Message);
                                Console.Write(e.Message);
                            }
                            finally
                            {
                                message.SwitchDirection();
                                message.DataTable = null;
                                message.Command.Operation = "deleteResponse";
                                UniCast(contextConnection, message);

                            }


                        }
                        #endregion
                        #region update
                        else if (message.Command.Operation == "update")
                        {
                            //UPDATE `nj_gps档案记录` SET `ID`=[value-1],`用户`=[value-2],`日期`=[value-3],`档案号`=[value-4],`坐标串`=[value-5] WHERE 1
                            try
                            {
                                sqlb.Append("UPDATE ");
                                if (!string.IsNullOrEmpty(message.DataTable.Database))
                                    sqlb.Append("`").Append(message.DataTable.Database).Append("`.");
                                sqlb.Append("`").Append(message.DataTable.TableName).Append("` SET ");
                                foreach (FoxundermoonLib.XmppEx.Data.Column c in message.DataTable.DataColumns)
                                {
                                    sqlb.Append(c.ColumnName).Append("=@").Append(c.ColumnName).Append(",");
                                }
                                sqlb.Remove(sqlb.Length - 1, 1).Append(" WHERE ").Append(message.Command.Condition);
                                var sql = sqlb.ToString();
                                var count = 0;
                                foreach (DataRow r in message.DataTable.Rows)
                                {
                                    int l = r.ItemArray.GetLength(0);
                                    MySqlParameter[] ps = new MySqlParameter[l];
                                    for (var i = 0; i < l; i++)
                                    {
                                        ps[i] = new MySqlParameter(message.DataTable.DataColumns[i].ColumnName, r.ItemArray[i]);
                                    }
                                    count += MysqlHelper.ExecuteNonQuery(sql, ps);
                                }
                                message.AddProperty("Count", count.ToString());
                            }
                            catch (Exception e)
                            {
                                message.AddProperty("error", "server error@MessageHandler DataTable update");
                                message.AddProperty("errorMessage", e.Message);
                                Console.Write(e.Message);
                            }
                            finally
                            {
                                message.SwitchDirection();
                                message.DataTable = null;
                                message.Command.Operation = "updateResponse";
                                UniCast(contextConnection, message);
                            }

                        }
                        #endregion
                        #region mutiQuery
                        var flag = true;
                        if (message.Command.Operation == "mutiquery")
                        {
                            #region 准备sql语句
                            if (string.IsNullOrEmpty(message.Command.Sql))
                            {
                                sqlb.Append("SELECT * FROM ");
                                if (!string.IsNullOrEmpty(message.DataTable.Database))
                                {
                                    sqlb.Append("`").Append(message.DataTable.Database).Append("`.");
                                }
                                if (string.IsNullOrEmpty(message.DataTable.TableName))
                                {
                                    message.Command.Name = Cmd.ErrorMessage;
                                    message.AddProperty("Message", "查询必须填写表名或者直接填写sql语句");
                                    message.SwitchDirection();
                                    UniCast(contextConnection, message);
                                    flag = false;
                                }
                                else
                                {
                                    sqlb.Append("`").Append(message.DataTable.TableName).Append("`");
                                    if (!string.IsNullOrEmpty(message.Command.Condition))
                                        sqlb.Append(" WHERE ").Append(message.Command.Condition);
                                }

                            }
                            else
                            {
                                sqlb.Append(message.Command.Sql);
                            }
                            #endregion
                            try
                            {
                                if (flag)
                                {
                                    var sql = sqlb.ToString();
                                    DataTable dt = null;
                                    foreach (DataRow r in message.DataTable.Rows)
                                    {
                                        int l = r.ItemArray.GetLength(0);
                                        MySqlParameter[] ps = new MySqlParameter[l];
                                        for (var i = 0; i < l; i++)
                                        {
                                            ps[i] = new MySqlParameter(message.DataTable.DataColumns[i].ColumnName, r.ItemArray[i]);
                                        }
                                        if (dt == null)
                                        {
                                            dt = MysqlHelper.ExecuteDataTable(sql, ps);
                                        }
                                        else
                                        {
                                            var appd = MysqlHelper.ExecuteDataTable(sql, ps);
                                            if (appd != null && appd.Rows.Count > 0)
                                            {
                                                foreach (DataRow _r in appd.Rows)
                                                {
                                                    dt.Rows.Add(dt.NewRow().ItemArray = _r.ItemArray);
                                                }
                                                appd.Clear();
                                                appd = null;
                                            }
                                        }
                                        message.setDataTable(dt);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                message.AddProperty("error", "server error@MessageHandler DataTable mutiquery");
                                message.AddProperty("errorMessage", e.Message);
                                Console.Write(e.Message);

                            }
                            finally
                            {
                                message.SwitchDirection();
                                message.Command.Operation = "mutiQueryResponse";
                                UniCast(contextConnection, message);
                            }


                        }


                        #endregion


                    }
                    #endregion
                    #region runsql
                    if (message.Command != null && message.Command.Operation == "runsql" && !string.IsNullOrEmpty(message.Command.Sql))
                    {
                        int count = 0;
                        try
                        {
                            count = MysqlHelper.ExecuteNonQuery(message.Command.Sql);
                            message.SwitchDirection();
                            message.Command.Operation = "runsqlResponse";
                            message.AddProperty("Count", count + "");
                        }
                        catch (Exception e)
                        {
                            message.AddProperty("error", "server error@MessageHandler DataTable runsql");
                            message.AddProperty("errorMessage", e.Message);
                            Console.Write(e.Message);

                        }
                        finally
                        {
                            UniCast(contextConnection, message);
                        }

                    }
                    #endregion
                    #region query
                    if (message.Command.Operation == "query" && !string.IsNullOrEmpty(message.Command.Sql))
                    {
                        try
                        {
                            DataTable dt = MysqlHelper.ExecuteDataTable(message.Command.Sql);
                            message.setDataTable(dt);
                        }
                        catch (Exception e)
                        {
                            message.AddProperty("error", "server error@MessageHandler DataTable query");
                            message.AddProperty("errorMessage", e.Message);
                            Console.Write(e.Message);
                        }
                        finally
                        {
                            message.Command.Operation = "queryResponse";
                            message.SwitchDirection();
                            UniCast(contextConnection, message);
                        }
                    }
                    #endregion
                }
                else if (dbt==databaseType.Sql)
                {
                    #region 有数据表操作
                    if (message.DataTable != null && message.DataTable.Rows.Count > 0)
                    {
                        var sqlb = new StringBuilder();
                        #region insert
                        if (message.Command.Operation == "insert")
                        {
                            bool hasLID = false;
                            foreach (DataColumn dc in message.DataTable.DataColumns)
                            {
                                if ("LID".Equals(dc.ColumnName))
                                    hasLID = true;
                            }
                            try
                            {
                                sqlb.Append("INSERT INTO ");
                                if (!string.IsNullOrEmpty(message.DataTable.Database))
                                {
                                    sqlb.Append(message.DataTable.Database).Append(".");
                                }
                                sqlb.Append(message.DataTable.TableName).Append("(");
                                var sbv = new StringBuilder();
                                foreach (FoxundermoonLib.XmppEx.Data.Column c in message.DataTable.DataColumns)
                                {
                                    sqlb.Append("").Append(c.ColumnName).Append(" , ");
                                    sbv.Append("@").Append(c.ColumnName).Append(",");
                                }
                                sqlb.Remove(sqlb.Length - 2, 2).Append(") VALUES (").Append(sbv.Remove(sbv.Length - 1, 1).Append(")").ToString());
                                var sql = sqlb.ToString();
                                var count = 0;
                                foreach (DataRow r in message.DataTable.Rows)
                                {
                                    int l = r.ItemArray.GetLength(0);
                                    SqlParameter[] ps = new SqlParameter[l];
                                    for (var i = 0; i < l; i++)
                                    {
                                        ps[i] = new SqlParameter(message.DataTable.DataColumns[i].ColumnName, r.ItemArray[i]);
                                    }
                                    count += SqlHelper.ExecteNonQueryText(sql, ps);
                                }
                                message.AddProperty("Count", count.ToString());
                                if (hasLID)
                                {
                                    wrapReturnTable(message,dbt);  //返回ID  LID 对应表
                                }

                            }
                            catch (Exception e)
                            {
                                message.AddProperty("error", "server error@MessageHandler DataTable insert");
                                message.AddProperty("errorMessage", e.Message);
                                Console.Write(e.Message);
                            }
                            finally
                            {
                                message.SwitchDirection();
                                message.Command.Operation = "insertResponse";
                                UniCast(contextConnection, message);
                            }
                            //INSERT INTO `nj_gps档案记录`(`ID`, `用户`, `日期`, `档案号`, `坐标串`) VALUES ([value-1],[value-2],[value-3],[value-4],[value-5])


                        }
                        #endregion
                        #region  delete
                        else if (message.Command.Operation == "delete")
                        {
                            try
                            {
                                sqlb.Append("DELETE FROM ");
                                if (!string.IsNullOrEmpty(message.DataTable.Database))
                                {
                                    sqlb.Append("[").Append(message.DataTable.Database).Append("].");
                                }
                                sqlb.Append("[").Append(message.DataTable.TableName).Append("] WHERE ")
                                    .Append(message.Command.Condition);
                                var sql = sqlb.ToString();
                                var count = 0;
                                foreach (DataRow r in message.DataTable.Rows)
                                {
                                    int l = r.ItemArray.GetLength(0);
                                    SqlParameter[] ps = new SqlParameter[l];
                                    for (var i = 0; i < l; i++)
                                    {
                                        ps[i] = new SqlParameter(message.DataTable.DataColumns[i].ColumnName, r.ItemArray[i]);
                                    }
                                    count += SqlHelper.ExecuteNonQuery(sql, ps);

                                }
                                message.AddProperty("Count", count.ToString());
                            }
                            catch (Exception e)
                            {
                                message.AddProperty("error", "server error@MessageHandler DataTable delete");
                                message.AddProperty("errorMessage", e.Message);
                                Console.Write(e.Message);
                            }
                            finally
                            {
                                message.SwitchDirection();
                                message.DataTable = null;
                                message.Command.Operation = "deleteResponse";
                                UniCast(contextConnection, message);

                            }


                        }
                        #endregion
                        #region update
                        else if (message.Command.Operation == "update")
                        {
                            //UPDATE `nj_gps档案记录` SET `ID`=[value-1],`用户`=[value-2],`日期`=[value-3],`档案号`=[value-4],`坐标串`=[value-5] WHERE 1
                            try
                            {
                                sqlb.Append("UPDATE ");
                                if (!string.IsNullOrEmpty(message.DataTable.Database))
                                    sqlb.Append("[").Append(message.DataTable.Database).Append("].");
                                sqlb.Append("[").Append(message.DataTable.TableName).Append("] SET ");
                                foreach (FoxundermoonLib.XmppEx.Data.Column c in message.DataTable.DataColumns)
                                {
                                    sqlb.Append(c.ColumnName).Append("=@").Append(c.ColumnName).Append(",");
                                }
                                sqlb.Remove(sqlb.Length - 1, 1).Append(" WHERE ").Append(message.Command.Condition);
                                var sql = sqlb.ToString();
                                var count = 0;
                                foreach (DataRow r in message.DataTable.Rows)
                                {
                                    int l = r.ItemArray.GetLength(0);
                                    SqlParameter[] ps = new SqlParameter[l];
                                    for (var i = 0; i < l; i++)
                                    {
                                        ps[i] = new SqlParameter(message.DataTable.DataColumns[i].ColumnName, r.ItemArray[i]);
                                    }
                                    count += SqlHelper.ExecuteNonQuery(sql, ps);
                                }
                                message.AddProperty("Count", count.ToString());
                            }
                            catch (Exception e)
                            {
                                message.AddProperty("error", "server error@MessageHandler DataTable update");
                                message.AddProperty("errorMessage", e.Message);
                                Console.Write(e.Message);
                            }
                            finally
                            {
                                message.SwitchDirection();
                                message.DataTable = null;
                                message.Command.Operation = "updateResponse";
                                UniCast(contextConnection, message);
                            }

                        }
                        #endregion
                        #region mutiQuery
                        var flag = true;
                        if (message.Command.Operation == "mutiquery")
                        {
                            #region 准备sql语句
                            if (string.IsNullOrEmpty(message.Command.Sql))
                            {
                                sqlb.Append("SELECT * FROM ");
                                if (!string.IsNullOrEmpty(message.DataTable.Database))
                                {
                                    sqlb.Append("[").Append(message.DataTable.Database).Append("].");
                                }
                                if (string.IsNullOrEmpty(message.DataTable.TableName))
                                {
                                    message.Command.Name = Cmd.ErrorMessage;
                                    message.AddProperty("Message", "查询必须填写表名或者直接填写sql语句");
                                    message.SwitchDirection();
                                    UniCast(contextConnection, message);
                                    flag = false;
                                }
                                else
                                {
                                    sqlb.Append("[").Append(message.DataTable.TableName).Append("]");
                                    if (!string.IsNullOrEmpty(message.Command.Condition))
                                        sqlb.Append(" WHERE ").Append(message.Command.Condition);
                                }

                            }
                            else
                            {
                                sqlb.Append(message.Command.Sql);
                            }
                            #endregion
                            try
                            {
                                if (flag)
                                {
                                    var sql = sqlb.ToString();
                                    DataTable dt = null;
                                    foreach (DataRow r in message.DataTable.Rows)
                                    {
                                        int l = r.ItemArray.GetLength(0);
                                        SqlParameter[] ps = new SqlParameter[l];
                                        for (var i = 0; i < l; i++)
                                        {
                                            ps[i] = new SqlParameter(message.DataTable.DataColumns[i].ColumnName, r.ItemArray[i]);
                                        }
                                        if (dt == null)
                                        {
                                            dt = SqlHelper.ExecuteDataTable(sql, ps);
                                        }
                                        else
                                        {
                                            var appd = SqlHelper.ExecuteDataTable(sql, ps);
                                            if (appd != null && appd.Rows.Count > 0)
                                            {
                                                foreach (DataRow _r in appd.Rows)
                                                {
                                                    dt.Rows.Add(dt.NewRow().ItemArray = _r.ItemArray);
                                                }
                                                appd.Clear();
                                                appd = null;
                                            }
                                        }
                                        message.setDataTable(dt);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                message.AddProperty("error", "server error@MessageHandler DataTable mutiquery");
                                message.AddProperty("errorMessage", e.Message);
                                Console.Write(e.Message);

                            }
                            finally
                            {
                                message.SwitchDirection();
                                message.Command.Operation = "mutiQueryResponse";
                                UniCast(contextConnection, message);
                            }


                        }


                        #endregion


                    }
                    #endregion
                    #region runsql
                    if (message.Command != null && message.Command.Operation == "runsql" && !string.IsNullOrEmpty(message.Command.Sql))
                    {
                        int count = 0;
                        try
                        {
                            count = SqlHelper.ExecuteNonQuery(message.Command.Sql);
                            message.SwitchDirection();
                            message.Command.Operation = "runsqlResponse";
                            message.AddProperty("Count", count + "");
                        }
                        catch (Exception e)
                        {
                            message.AddProperty("error", "server error@MessageHandler DataTable runsql");
                            message.AddProperty("errorMessage", e.Message);
                            Console.Write(e.Message);

                        }
                        finally
                        {
                            UniCast(contextConnection, message);
                        }

                    }
                    #endregion
                    #region query
                    if (message.Command.Operation == "query" && !string.IsNullOrEmpty(message.Command.Sql))
                    {
                        try
                        {
                            DataTable dt = SqlHelper.ExecuteDataTable(message.Command.Sql,null);
                            message.setDataTable(dt);
                        }
                        catch (Exception e)
                        {
                            message.AddProperty("error", "server error@MessageHandler DataTable query");
                            message.AddProperty("errorMessage", e.Message);
                            Console.Write(e.Message);
                        }
                        finally
                        {
                            message.Command.Operation = "queryResponse";
                            message.SwitchDirection();
                            UniCast(contextConnection, message);
                        }
                    }
                    #endregion

                }



            #endregion

                #region 获取在线用户
                if (FoxundermoonLib.XmppEx.Command.Cmd.GetOnlineUsers.Equals(message.Command.Name))
                {
                    try
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("UserName");
                        dt.Columns.Add("Resource");
                        foreach (KeyValuePair<string, ConcurrentDictionary<string, agsXMPP.XmppSeverConnection>> item in XmppConnectionDic)
                        {
                            foreach (KeyValuePair<string, agsXMPP.XmppSeverConnection> con in item.Value)
                            {
                                var row = dt.NewRow();
                                row["UserName"] = item.Key;
                                row["Resource"] = con.Key;
                                dt.Rows.Add(row);
                            }

                        }
                        message.setDataTable(dt);
                    }
                    catch (Exception e)
                    {
                        message.AddProperty("error", "server error@MessageHandler getOnlineUser ");
                        message.AddProperty("errorMessage", e.Message);
                    }
                    finally
                    {
                        message.Command.Name = Cmd.GetOnlineUsersResponse;
                        message.SwitchDirection();
                        UniCast(contextConnection, message);
                    }


                }
            }
            catch (Exception e)
            {
                message.AddProperty("error", "server error@MessageHandler Datatable outer ");
                message.AddProperty("errorMessage", e.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("server error@MessageHandler Datatable outer " + e.Message);
                try
                {
                    UniCast(contextConnection, message);
                }
                catch { }
            }
        }
                #endregion


        private static void wrapReturnTable(FoxundermoonLib.XmppEx.Data.Message message,databaseType dbType)
        {
            databaseType dbt = dbType;

            var sbin = new StringBuilder();
            foreach (DataRow r in message.DataTable.Rows)
            {
                sbin.Append(r["LID"]).Append(",");
            }
            sbin.Remove(sbin.Length - 1, 1);
            string retSql = string.Format("select ID,LID from {0} where LID in ({1})", message.DataTable.TableName, sbin.ToString());
            string delSql = string.Format("UPDATE  {0}. {1}  SET LID=-1  WHERE LID in ({2})", message.DataTable.Database, message.DataTable.TableName, sbin.ToString());
            DataTable retTable = null;
            if (dbt == databaseType.MySql)
                retTable = MysqlHelper.ExecuteDataTable(retSql);
            else if (dbt == databaseType.Sql)
                retTable = SqlHelper.ExecuteDataTable(retSql,null);
            if (retTable != null && retTable.Rows.Count > 0)
            {
                message.setDataTable(retTable);
                if (dbt == databaseType.MySql)
                    MysqlHelper.ExecuteNonQuery(delSql);
                else if (dbt == databaseType.Sql)
                    SqlHelper.ExecuteNonQuery(delSql);

            
            }
            else
            {
                message.AddProperty("error", "server error@MessageHandler return insert");
                message.AddProperty("errorMessage", "无法返回数据库id  插入错误或者服务器端错误");
            }
        }

        private static databaseType getDatabaseType(FoxundermoonLib.XmppEx.Data.Message msg)
        {
            if (string.IsNullOrEmpty(msg.Command.Name))
                return databaseType.None;
            else if (Cmd.SqlDataTable.Equals(msg.Command.Name))
                return databaseType.Sql;
            else if (Cmd.MySqlDataTable.Equals(msg.Command.Name) || Cmd.DataTable.Equals(msg.Command.Name))
                return databaseType.MySql;
            return databaseType.None;

        }
        enum databaseType
        {
            Sql,
            MySql,
            None,
        }
    }
}
