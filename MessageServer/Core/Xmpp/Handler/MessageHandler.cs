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
            if (msg.To != null &&  !string.IsNullOrEmpty(msg.To.User))
            {
                message.ToUser =msg.To.User;
            }

            if (msg.From != null && msg.From.User != null)
            {
                message.FromUser = msg.From.User;
            }
            command = msg.Subject;

            //转发 message
            // to != "0" and ""
       
            message.SetJsonMessage(content);
            message.SetJsonCommand(command);
            if (!string.IsNullOrEmpty(message.ToUser) && message.ToUser != "0" )
            {
                UniCast(message);
            }
            #endregion
            Console.WriteLine(message.ToJson(true));
            #region  数据表操作
            if (Cmd.DataTable.Equals(message.Command.Name))
            {
                #region 有数据表操作
                if (message.DataTable != null && message.DataTable.Rows.Count > 0)
                {
                    var sqlb = new StringBuilder();
                    #region insert
                    if (message.Command.Operation == "insert")
                    {
                        //INSERT INTO `nj_gps档案记录`(`ID`, `用户`, `日期`, `档案号`, `坐标串`) VALUES ([value-1],[value-2],[value-3],[value-4],[value-5])
                        sqlb.Append("INSERT INTO ");
                        if (!string.IsNullOrEmpty(message.DataTable.Database))
                        {
                            sqlb.Append("`").Append(message.DataTable.Database).Append("`.");
                        }
                        sqlb.Append("`").Append(message.DataTable.TableName).Append("`(");
                        var sbv = new StringBuilder();
                        foreach (FoxundermoonLib.XmppEx.Data.Column c in message.DataTable.DataColumns)
                        {
                            sqlb.Append("`").Append(c.ColumnName).Append("` , ");
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
                        message.SwitchDirection();
                        message.DataTable = null;
                        message.Command.Operation = "insertResponse";
                        message.AddProperty("Count", count.ToString());
                        UniCast(contextConnection, message);
                    }
                    #endregion
                    #region  delete
                    else if (message.Command.Operation == "delete")
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
                        message.SwitchDirection();
                        message.DataTable = null;
                        message.Command.Operation = "deleteResponse";
                        message.AddProperty("Count", count.ToString());
                        UniCast(contextConnection, message);
                    }
                    #endregion
                    #region update
                    else if (message.Command.Operation == "update")
                    {
                        //UPDATE `nj_gps档案记录` SET `ID`=[value-1],`用户`=[value-2],`日期`=[value-3],`档案号`=[value-4],`坐标串`=[value-5] WHERE 1
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
                        message.SwitchDirection();
                        message.DataTable = null;
                        message.Command.Operation = "updateResponse";
                        message.AddProperty("Count", count.ToString());
                        UniCast(contextConnection, message);
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

                            }
                            message.setDataTable(dt);
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
                    MysqlHelper.ExecuteNonQuery(message.Command.Sql);
                    message.SwitchDirection();
                    message.Command.Operation = "runsqlResponse";
                    UniCast(contextConnection, message);
                }
#endregion
                #region query
                if (message.Command.Operation == "query" && !string.IsNullOrEmpty(message.Command.Sql))
                {
                    try
                    {

                        DataTable dt = MysqlHelper.ExecuteDataTable(message.Command.Sql);
                        message.setDataTable(dt);
                        message.SwitchDirection();
                        message.Command.Operation = "queryResponse";
                        UniCast(contextConnection, message);
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                    }
                }
                #endregion
            }

            #endregion

            #region 获取在线用户
            if (FoxundermoonLib.XmppEx.Command.Cmd.GetOnlineUsers.Equals(message.Command.Name))
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("UserName");
                foreach (var item in XmppConnectionDic)
                {
                    var row = dt.NewRow();
                    row["UserName"] = item.Key;
                    dt.Rows.Add(row);
                }
                message.setDataTable(dt);
                message.Command.Name = Cmd.GetOnlineUsersResponse;
                UniCast(contextConnection, message);
            }
            #endregion
        }
    }
}
