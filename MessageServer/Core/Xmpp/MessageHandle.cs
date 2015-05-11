using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using agsXMPP;
using agsXMPP.Xml;
using agsXMPP.protocol.client;
using agsXMPP.Xml.Dom;
namespace FileDownloadAndUpload.Core.Xmpp
{
    public class MessageHandle
    {
        FileDownloadAndUpload.Models.Entities mentiti;
        Dictionary<int, XmppSeverConnection> _conDic;
        public MessageHandle(Dictionary<int, XmppSeverConnection> conDic)
        {
            _conDic = conDic;
            mentiti = new FileDownloadAndUpload.Models.Entities();
        }

        public void Handle(XmppSeverConnection con, Message msg)
        {
            if ( true)
                HandleServerMsg(con,msg);
            else
            {
                try
                {
                    
                    int fromUid = int.Parse(msg.From.User);

                    int toUid = int.Parse(msg.To.User);
                    if (!_conDic.ContainsKey(fromUid))
                        _conDic.Add(fromUid, con);  //以发送者的uid作为key存储发送者与服务器之间的连接
                    Models.Message msgModel = mentiti.Message.Create(); //数据库实体
                    msgModel.From = int.Parse(msg.From.User);
                    msgModel.To = int.Parse(msg.To.User);
                    XmppSeverConnection tmpCon;  //根据uid  选择对应的 connection 转发数据
                    if (_conDic.TryGetValue(toUid, out tmpCon))
                    {
                        tmpCon.Send(msg);
                        msgModel.Status = 1; //已发送
                    }
                    else
                        msgModel.Status = 0; //未发送
                    mentiti.Message.Add(msgModel);

                    try
                    {
                        //保存入库
                        mentiti.SaveChanges();
                    }
                    catch (Exception e)
                    {

                    }

                }
                catch (Exception ae)
                {

                }
            }

        }


        //处理客户端与服务器的通信   在此处加入服务器相关业务逻辑代码
        private void HandleServerMsg(XmppSeverConnection con,Message msg)
        {
            if(msg.Body == "123456")
            {
                Message m = new Message();
                m.Body = "-----------------------";
                m.From = new Jid("1@localhost");
                m.To = msg.From;
                con.Send(m);
            }
        }
    }
}