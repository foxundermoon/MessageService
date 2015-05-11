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
using MessageService.Core.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace MessageService.Core.Xmpp {
   partial class XmppServer {
       public void Broadcast( string strMsg, Xmpp.Type type ) {
           if(type == Xmpp.Type.Message) {
               Message msg = new Message();
               msg.From = new Jid("0@10.80.5.222/Server");
               foreach(var con in XmppConnectionDic) {
                   Jid to = new Jid(con.Key + "@10.80.5.222");
                   msg.To = to;
                   con.Value.Send(msg);
               }

           }
           if(type == Xmpp.Type.Notification) {

               IQ notificationIQ = new IQ();
               Element notify = new Element();
               notify.Namespace = "androidpn:iq:notification";
               notify.TagName = "notification";
               notify.AddChild(new Element("id", Guid.NewGuid().ToString()));
               notificationIQ.AddChild(notify);
               notificationIQ.From = new Jid("0@10.80.5.222/Server");

               foreach(var con in XmppConnectionDic) {
                   Jid to = new Jid(con.Key + "@10.80.5.222");
                   notificationIQ.To = to;
                   con.Value.Send(notificationIQ);
               }


               //<iq xmlns="jabber:client" from="0@10.80.5.222/Server">
               //<notification xmlns="androidpn:iq:notification">
               //<id>3fada5a4-3f2e-4652-bf8c-01bb4df0debb</id>
               //</notification>
               //</iq>

           }
       }
       /// <summary>
       /// 广播
       /// </summary>
       /// <param name="strMsg">广播的消息</param>
       public void Broadcast( string strMsg ) {
           Message msg = new Message();
           msg.From = new Jid("0@10.80.5.222/Server");
           msg.Body = strMsg;
           foreach(var con in XmppConnectionDic) {
               Jid to = new Jid(con.Key + "@10.80.5.222");
               msg.To = to;
               con.Value.Send(msg);
           }
       }
       /// <summary>
       /// 单播
       /// </summary>
       /// <param name="id">客户端  或者用户id</param>
       /// <param name="strMsg">发送的数据</param>
       public void Unicast( string id, string strMsg ) {
           if(XmppConnectionDic.ContainsKey(id)) {
               XmppSeverConnection value;
               if(XmppConnectionDic.TryGetValue(id, out value)) {
                   Message msg = new Message();
                   msg.To = new Jid(id + "@10.80.5.222");
                   msg.Body = strMsg;
                   value.Send(msg);
               }

           }
       }

       internal void Broadcast( agsXMPP.protocol.Base.Stanza reply ) {
           foreach(var con in XmppConnectionDic) {
               Jid to = new Jid(con.Key + "@10.80.5.222");
               reply.To = to;
               con.Value.Send(reply);
           }
       }
    }
}
