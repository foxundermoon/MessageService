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
            msg.From = new Jid(string.IsNullOrEmpty(message.FromUser) ? "0" : message.FromUser + "@" + Config.ServerIp);
            msg.Body = FoxundermoonLib.Encrypt.EncryptUtil.EncryptBASE64ByGzip(message.ToJson());
            msg.Subject = message.GetJsonCommand();
            foreach (var con in XmppConnectionDic)
            {
                Jid to = new Jid(con.Key + "@" + Config.ServerIp);
                msg.To = to;
                con.Value.Send(msg);
            }

        }

        public void UniCast(FoxundermoonLib.XmppEx.Data.Message message)
        {
            Message msg = new Message();
            msg.From = new Jid(string.IsNullOrEmpty(message.FromUser) ? "0" : message.FromUser + "@" + Config.ServerIp);
            if (string.IsNullOrEmpty(message.ToUser))
                throw new Exception("not set ToUser in the message");
            msg.To = new Jid(message.ToUser + "@" + Config.ServerIp);

            var hasCon = XmppConnectionDic.Keys.Contains(message.ToUser);
            if (!hasCon)
                throw new Exception("the user are not online");
            XmppSeverConnection con =null;
            if (XmppConnectionDic.TryGetValue(message.ToUser, out con))
                con.Send(msg);
        }
    }
}
