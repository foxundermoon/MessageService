using System;
using System.Linq;
using MessageService;
using System.Diagnostics;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.auth;
using agsXMPP.Xml.Dom;
using MessageService.Core.Account;
using agsXMPP.protocol.iq.roster;
using agsXMPP;

namespace MessageService.Core.Xmpp
{
    public partial class XmppServer
    {
        public async void OnIQ(XmppSeverConnection contextConnection, IQ iq)
        {
            ProcessIQAsync(contextConnection, iq);
        }
        private void ProcessIQAsync(agsXMPP.XmppSeverConnection contextConnection, IQ iq)
        {
            if (iq.Query.GetType() == typeof(Auth))
            {
                Auth auth = iq.Query as Auth;
                switch (iq.Type)
                {
                    case IqType.get:
                        iq.SwitchDirection();
                        iq.Type = IqType.result;
                        auth.AddChild(new Element("password"));
                        //auth.AddChild(new Element("digest"));
                        Console.WriteLine(auth.Username + " :开始登陆!");
                        contextConnection.Send(iq);
                        break;
                    case IqType.set:
                        // Here we should verify the authentication credentials
                        Console.WriteLine(auth.Username + " : " + "开始验证, 密码：" + auth.Password);
                        iq.SwitchDirection();
                        if (AccountBus.CheckAccountAsync(auth.Username, auth.Password))  //验证用户是否存在或者密码是否正确
                        {
                            contextConnection.IsAuthentic = true;
                            iq.Type = IqType.result;
                            iq.Query = null;
                            try
                            {
                                string uid = (auth.Username);
                                //Func<int,XmppSeverConnection,XmppSeverConnection> update = (k,v)=>{return v;};
                                //XmppConnectionDic.AddOrUpdate(uid, contextConnection),(k,v)=>{return v;});

                                if (XmppConnectionDic.ContainsKey(uid))
                                {
                                    FoxundermoonLib.XmppEx.Data.Message offLine = new FoxundermoonLib.XmppEx.Data.Message();
                                    offLine.ToUser = uid;
                                    offLine.Command.Name = FoxundermoonLib.XmppEx.Command.Cmd.OnLineAtOtherPlace;

                                    XmppSeverConnection _;
                                    if (XmppConnectionDic.TryRemove(uid, out _))
                                    {
                                        UniCast(_, offLine);
                                        Console.WriteLine(uid + " 重新登录");
                                    }
                                }
                                if (XmppConnectionDic.TryAdd(uid, contextConnection))
                                {
                                    Console.WriteLine(auth.Username + ": 账号验证成功,并加入连接池！");
                                }

                                UserOnline(uid);
                                contextConnection.UserName = uid;
                            }
                            catch (Exception e)
                            {
                                // 消息没有 From    dosomething
                                iq.Type = IqType.error;
                                iq.Value = e.Message;
                                Console.WriteLine("Exception --> message: " + e.Message + "  data:" + e.Data);
                            }
                        }
                        else
                        {
                            // authorize failed
                            iq.Type = IqType.error;  //若要开启验证功能去掉此注释
                            Console.WriteLine(auth.Username + ":账号验证失败!");
                            FoxundermoonLib.XmppEx.Data.Message loginFailed = new FoxundermoonLib.XmppEx.Data.Message();
                            loginFailed.Command.Name = FoxundermoonLib.XmppEx.Command.Cmd.ErrorMessage;
                            loginFailed.AddProperty("Cause", "账号验证失败，请检查用户名或者密码");
                            loginFailed.ToUser = auth.Username;
                            UniCast(loginFailed);
                            //iq.Type = IqType.result;
                            iq.Query = null;
                            iq.Value = "authorized failed";
                            contextConnection.IsAuthentic = false;
                        }
                        contextConnection.Send(iq);
                        break;
                }
            }
            else if (!contextConnection.IsAuthentic)
            {
                contextConnection.Stop();
            }
            else if (iq.Query.GetType() == typeof(Roster))
            {
                ProcessRosterIQ(contextConnection, iq);
            }

        }

        private void ProcessRosterIQ(agsXMPP.XmppSeverConnection contextConnection, IQ iq)
        {
            if (iq.Type == IqType.get)
            {
                // Send the roster
                // we send a dummy roster here, you should retrieve it from a
                // database or some kind of directory (LDAP, AD etc...)
                iq.SwitchDirection();
                iq.Type = IqType.result;
                for (int i = 1; i < 11; i++)
                {
                    RosterItem ri = new RosterItem();
                    ri.Name = "Item " + i.ToString();
                    ri.Subscription = SubscriptionType.both;
                    ri.Jid = new Jid("item" + i.ToString() + "@localhost");
                    ri.AddGroup("localhost");
                    iq.Query.AddChild(ri);
                }
                RosterItem ri1 = new RosterItem();
                for (int i = 1; i < 11; i++)
                {
                    RosterItem ri = new RosterItem();
                    ri.Name = "Item JO " + i.ToString();
                    ri.Subscription = SubscriptionType.both;
                    ri.Jid = new Jid("item" + i.ToString() + "@jabber.org");
                    ri.AddGroup("JO");
                    iq.Query.AddChild(ri);
                }
                contextConnection.Send(iq);
            }
        }
    }
}
