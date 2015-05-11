using System;
using System.Linq;
using System.Web;
using MessageService;
using System.Diagnostics;
using agsXMPP.protocol.client;
using agsXMPP;

namespace MessageService.Core.Xmpp
{
    public partial class XmppServer
    {
        public void OnPresence(agsXMPP.XmppSeverConnection contextConnection, Presence presence)
        {
            if (contextConnection.IsAuthentic)
            {
                processPresence(contextConnection, presence);
            }
            else
            {
                contextConnection.Stop();
            }
        }
        /// <summary>
        ///  route presences here and handle all subscription stuff
        /// </summary>
        /// <param name="node"></param>
        private void processPresence(agsXMPP.XmppSeverConnection contextConnection, Presence presence)
        {

            //Trace.Write(node.ToString(), HandlerType.FullName);
            // route presences here and handle all subscription stuff
            ///登录
            if (presence.Type == PresenceType.subscribe)
            {
                if (presence.Status == "online")
                {
                    try
                    {
                        //string pswd = presence.GetTag("passwd");
                        int uid = Convert.ToInt32(presence.From.User);
                            //if (XmppConnectionDic.ContainsKey(uid))
                            //{
                            //    XmppConnectionDic.Remove(uid);
                            //}
                            //XmppConnectionDic.Add(uid, contextConnection);
                            contextConnection.IsAuthentic = true;
                            Presence reply = new Presence();
                            reply.From = ServerJid;
                            reply.To = presence.From;
                            reply.Status = "onlined";
                            reply.Type = PresenceType.subscribed;
                            reply.Id = presence.Id;
                            reply.Value = "ok";
                            contextConnection.Send(reply);
                            Broadcast(presence);
                        //}
                        //else
                        //{
                        //    presence.Error = new Error(ErrorCondition.NotAuthorized);
                        //    presence.Value = "error password";
                        //    presence.SwitchDirection();
                        //    contextConnection.Send(presence);
                        //    contextConnection.Stop();
                        //}
                    }
                    catch (Exception e)
                    {
                        presence.Error = new Error(ErrorCondition.BadRequest);
                        presence.Value = e.Message + e.ToString();
                        presence.SwitchDirection();
                        contextConnection.Send(presence);
                        contextConnection.Stop();
                    }
                }
            }
            ///注销用户
            else if (presence.Type == PresenceType.unsubscribe)
            {
                string uid = (presence.From.User);
                if (XmppConnectionDic.ContainsKey(uid))
                {
                    XmppSeverConnection _;
                    XmppConnectionDic.TryRemove(uid,out _);
                }
                Broadcast(presence);
                presence.Type = PresenceType.unsubscribed;
                contextConnection.Send(presence);
            }
            else if (presence.Type == PresenceType.available)
            {
                if (presence.Status == "ping")
                {
                    presence.Type = PresenceType.available;
                    presence.Status = "pinged";
                    presence.SwitchDirection();
                    contextConnection.Send(presence);
                }
            }
        }
    }
}
