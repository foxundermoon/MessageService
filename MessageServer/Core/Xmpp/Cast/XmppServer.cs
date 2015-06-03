using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.Xml.Dom;
using System.Collections.Concurrent;
using System.Configuration;
using MessageService.Core.Config;
using System.Threading.Tasks;
using agsXMPP;
namespace MessageService.Core.Xmpp
{
    partial class XmppServer
    {
        /// <summary>
        /// 广播
        /// </summary>
        /// <param name="strMsg">广播的消息</param>
        public void Broadcast(FoxundermoonLib.XmppEx.Data.Message message)
        {
            Message msg = new Message();
            if (null != message.FromUser)
                msg.From = getJidFromUser(message.FromUser);
            else
                msg.From = getJidFromUser(new FoxundermoonLib.XmppEx.Data.User("0","server"));
            msg.Body = FoxundermoonLib.Encrypt.EncryptUtil.EncryptBASE64ByGzip(message.ToJson());
            msg.Subject = message.GetJsonCommand();
            msg.Language = "BASE64";

            foreach (var cons in XmppConnectionDic)
            {
                foreach (var con in cons.Value)
                {

                    Jid to = new Jid(cons.Key + "@" + Config.ServerIp + "/" + con.Key);
                    msg.To = to;
                    try
                    {
                        con.Value.Send(msg);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception@XmppServer.Broadcast message:" + e.Message);
                    }
                }

            }
        }
        public void Broadcast2resource(FoxundermoonLib.XmppEx.Data.Message message, string resource)
        {
            Message msg = new Message();
            if (null == message.FromUser)
                msg.From = getJidFromUser(message.FromUser);
            else
                msg.From = getJidFromUser(message.FromUser);
            msg.Body = FoxundermoonLib.Encrypt.EncryptUtil.EncryptBASE64ByGzip(message.ToJson());
            msg.Subject = message.GetJsonCommand();
            msg.Language = "BASE64";

            foreach (var cons in XmppConnectionDic)
            {
                XmppSeverConnection  con=null;
                var hasCon = cons.Value.TryGetValue(resource, out con);
                if(hasCon)
                {

                    Jid to = new Jid(cons.Key + "@" + Config.ServerIp + "/" + resource);
                    msg.To = to;
                    try
                    {
                        con.Send(msg);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception@XmppServer.Broadcast message:" + e.Message);
                    }
                }
            }
        }

        public void UniCast(FoxundermoonLib.XmppEx.Data.Message message)
        {
            if (null == message.ToUser || string.IsNullOrEmpty(message.ToUser.Name))
                throw new Exception("not set ToUser in the message");
            if (string.IsNullOrEmpty(message.ToUser.Resource))
                throw new Exception("not set Resource");
            ConcurrentDictionary<string, XmppSeverConnection> cons = null;
            var hasCons = XmppConnectionDic.TryGetValue(message.ToUser.Name, out cons);
            if (!hasCons)
                throw new Exception("the user are not online");
            XmppSeverConnection con = null;
            var hasCon = cons.TryGetValue(message.ToUser.Resource, out con);
            if (!hasCon)
                throw new Exception("the user of the resource are not onling");
            UniCast(con, message);
        }
        public void UniCast(XmppSeverConnection contexCon, FoxundermoonLib.XmppEx.Data.Message message)
        {
            Message msg = new Message();
            if (null == message.FromUser)
                msg.From = ServerJid;
            else
                msg.From = getJidFromUser(message.FromUser);
            if (null != message.ToUser)
            {
                msg.To = getJidFromUser(message.ToUser);
            }
            msg.Language = "BASE64";
            msg.Subject = message.GetJsonCommand();
            msg.Body = FoxundermoonLib.Encrypt.EncryptUtil.EncryptBASE64ByGzip(message.ToJson());
            try
            {
                contexCon.Send(msg);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception@Xmppserver.UniCast" + e.Message);
            }
        }

        public Jid getJidFromUser(FoxundermoonLib.XmppEx.Data.User u)
        {
            return new Jid(u.Name + "@" + Config.ServerIp + "/" + u.Resource);
        }
    }
}
