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

            #region 转换Message
            var content = "";
            var fromUser = "";
            var toUser = "";
            var resource = "";
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
            if (msg.To != null && msg.To.User != null)
            {
                toUser = msg.To.User;
            }

            if (msg.From != null && msg.From.User != null)
            {
                resource = msg.From.Resource;
                fromUser = msg.From.User;
            }
            command = msg.Subject;

            //转发 message
            // to != "0" and ""
            if (toUser != "0" && toUser.Length > 0)
            {
                XmppSeverConnection connection;
                if (XmppConnectionDic.TryGetValue(toUser, out connection))
                {
                    connection.Send(msg);
                }
            }
            FoxundermoonLib.XmppEx.Data.Message message = new FoxundermoonLib.XmppEx.Data.Message();
            message.SetJsonMessage(content);
            message.SetJsonCommand(command);
            #endregion
            Console.WriteLine(message.ToJson(true));
            #region  数据表操作
            if ("datatable".Equals(message.Command.Name))
            {
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

                        foreach (DataRow r in message.DataTable.Rows)
                        {
                            int l = r.ItemArray.GetLength(0);
                            MySqlParameter[] ps = new MySqlParameter[l];
                            for (var i = 0; i < l; i++)
                            {
                                ps[i] = new MySqlParameter(message.DataTable.DataColumns[i].ColumnName, r.ItemArray[i]);
                            }
                            MysqlHelper.ExecuteNonQuery(sql, ps);
                        }

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
                        foreach (DataRow r in message.DataTable.Rows)
                        {
                            int l = r.ItemArray.GetLength(0);
                            MySqlParameter[] ps = new MySqlParameter[l];
                            for (var i = 0; i < l; i++)
                            {
                                ps[i] = new MySqlParameter(message.DataTable.DataColumns[i].ColumnName, r.ItemArray[i]);
                            }
                            MysqlHelper.ExecuteNonQuery(sql, ps);
                        }
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
                        foreach (DataRow r in message.DataTable.Rows)
                        {
                            int l = r.ItemArray.GetLength(0);
                            MySqlParameter[] ps = new MySqlParameter[l];
                            for (var i = 0; i < l; i++)
                            {
                                ps[i] = new MySqlParameter(message.DataTable.DataColumns[i].ColumnName, r.ItemArray[i]);
                            }
                            MysqlHelper.ExecuteNonQuery(sql, ps);
                        }
                    }
                    #endregion

                }
                #region runsql
                if (message.Command != null && message.Command.Operation == "runsql" && !string.IsNullOrEmpty(message.Command.Sql))
                {
                    MysqlHelper.ExecuteNonQuery(message.Command.Sql);

                }
                if (message.Command != null && message.Command.Operation == "query" && !string.IsNullOrEmpty(message.Command.Sql))
                {
                    try
                    {

                        DataTable dt = MysqlHelper.ExecuteDataTable(message.Command.Sql);
                        message.setDataTable(dt);
                        msg.SwitchDirection();
                        message.Command.Name = "callback";
                        msg.Body = EncryptUtil.EncryptBASE64ByGzip(message.ToJson());
                        msg.Subject = message.GetJsonCommand();
                        contextConnection.Send(msg);
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
