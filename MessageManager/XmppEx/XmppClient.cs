﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using agsXMPP;
using agsXMPP.protocol.client;
using System.Threading;
namespace XmppEx
{
    public class XmppClient
    {
        public bool IsRunDaemon { get; set; }
        public Jid LocalJid { get; set; }
        public Jid ServerJid { get; set; }
        public string Name { get; set; }
        public Thread DaemonThread { get; set; }
        private PingRequest pingRequest;
        private PingResponse pingRespose;
        public string Password { get; set; }
        public bool IsLongined { get; set; }
        public int pinginterval;
        public event ObjectHandler OnLogin;
        public event MessageHandler OnMessage;
        public event PresenceHandler OnPresence;
        public event IqHandler OnIQ;
        public event ErrorHandler OnXmppError;
        public XmppClientConnection XmppConnection;
        static private XmppClient instance;
        static object _lock = new object();
        public static XmppClient Instance
        {
            get
            {
                lock (_lock)
                {
                    if (instance == null)
                        instance = new XmppClient();
                    return instance;
                }
            }
        }
        static private object locked = new object();
        public DataHolder holder = new DataHolder();
        public string ServerResource { get; set; }
        private XmppClient()
        {
            pinginterval = 20; //int.Parse(System.Configuration.ConfigurationManager.AppSettings["pinginterval"].ToString());
            //ServerJid = new Jid("0", "10.80.5.222", "server");
            holder.CurrentConnectCount = 0;
            holder.ReConnectCount = 5; //int.Parse(System.Configuration.ConfigurationManager.AppSettings["reconnectcount"].ToString());
        }

        public static XmppClient CreatNewInstance()
        {
            return new XmppClient();
        }
        public static XmppClient GetInstance()
        {
            return Instance;

        }
        public void StartDaemon()
        {
            IsRunDaemon = true;
            ThreadStart ts = new ThreadStart(_startDaemon);
            DaemonThread = new Thread(ts);
            //DaemonThread.Start();
        }
        private void check()
        {
            sendPing();
            Thread.Sleep(pinginterval * 1000);
            if (pingRespose.PingID.Trim() != pingRequest.PingID.Trim())
            {
                IsLongined = false;
                ErrorEvent args = new ErrorEvent("连接丢失!,开始第" + holder.CurrentConnectCount + "次重连");
                OnXmppError(args);
                if (holder.CanReconnect)
                {
                    reConnect();
                }
                else
                {
                    ErrorEvent failed = new ErrorEvent("登录失败 重试次数已满");
                    OnXmppError(failed);
                }
            }
        }
        private void _startDaemon()
        {
            while (IsRunDaemon)
            {
                try
                {
                    check();
                }
                catch (Exception e)
                {
                    IsLongined = false;
                    if (holder.CanReconnect)
                    {

                        ErrorEvent args = new ErrorEvent("数据连接错误!" + holder.CurrentConnectCount + "次重连, 错误消息:" + e.Message);
                        OnXmppError(args);
                        reConnect();
                    }
                    else
                    {
                        ErrorEvent failed = new ErrorEvent("登录失败 重试次数已满");
                        OnXmppError(failed);
                    }
                }
            }
        }

        private void reConnect()
        {
            holder.CurrentConnectCount++;
            XmppConnection.Close();
            Login();
            //_startDaemon();
        }
        private void subscrib()
        {
            Presence sub = new Presence();
            sub.Status = "online";
            sub.Type = PresenceType.subscribe;
            holder.Id = Guid.NewGuid().ToString();
            sub.Id = holder.Id;
            sub.To = ServerJid;
            sub.From = LocalJid;
            XmppConnection.Send(sub);
        }
        private void sendPing()
        {

            string tmpID = Guid.NewGuid().ToString();
            pingRequest = new PingRequest();
            pingRequest.PingID = tmpID;
            pingRequest.StartTime = DateTime.UtcNow.Ticks;
            Presence heart = new Presence();
            heart.Id = tmpID;
            heart.To = ServerJid;
            heart.From = LocalJid;
            heart.Type = PresenceType.available;
            heart.SetTag("ping", pingRequest.StartTime);
            heart.Status = "ping";
            XmppConnection.Send(heart);
        }
        public void Login()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new ClientException("用户id不能为空!");
            }
            if (string.IsNullOrWhiteSpace(Password))
                throw new ClientException("Password is empty!");
            XmppConnection = new XmppClientConnection(LocalJid.Server);
            XmppConnection.OnLogin += new ObjectHandler(XmppClient_OnLogin);
            XmppConnection.Port = ServerPort;
            XmppConnection.OnMessage += new MessageHandler(XmppConnection_OnMessage);
            XmppConnection.OnPresence += new PresenceHandler(XmppConnection_OnPresence);
            XmppConnection.OnIq += new IqHandler(XmppConnecion_OnIQ);
            XmppConnection.OnError += XmppConnection_OnError;
            //new ErrorHandler(XmppConnection_OnError);
            XmppConnection.Open(LocalJid.User, Password,ServerResource);
        }

        void XmppConnection_OnError(object sender, Exception ex)
        {
            OnXmppError(new ErrorEvent(ex.Message));
        }

        private void XmppConnecion_OnIQ(object sender, IQ iq)
        {
            if (OnIQ != null)
                OnIQ(sender, iq);
        }
        void XmppClient_XmppErrorHandler(object sender, ErrorEvent msg)
        {
            if (msg.ErrT == ErrorEvent.ErrorType.AuthernitedFailed)
            {
                IsRunDaemon = false;
            }
        }
        void XmppClient_OnLogin(object sender)
        {
            IsLongined = true;
            holder.CurrentConnectCount = 0;
            if (OnLogin != null)
                OnLogin(this);
        }
        void XmppConnection_OnPresence(object sender, Presence pres)
        {
            if (OnPresence != null)
                OnPresence(sender, pres);
            if (pres.Type == PresenceType.available)
            {
                if (pres.HasTag("ping-response") || pres.Status == "pinged")
                {
                    pingRespose.PingID = pres.Id;
                    pingRespose.ResponseTime = DateTime.UtcNow.Ticks;
                }
                else if (pres.HasTag("ping"))
                {
                    if (pres.Error != null)
                    {
                        OnXmppError(new ErrorEvent("认证失败:" + pres.Error.Type + pres.Error.Value));
                        IsLongined = false;
                    }
                }
            }
            else if (pres.Type == PresenceType.subscribed)
            {
                if (pres.Id == holder.Id)
                {
                    if (pres.Value == "ok")
                    {
                        IsLongined = true;
                        holder.Subscribed = true;
                        holder.DateTime = DateTime.UtcNow.Ticks;
                        //XmppClient_OnLogin(this);
                        //OnLogin.Invoke(this);
                    }
                }
            }
        }
        void XmppConnection_OnMessage(object sender, Message msg)
        {
            if (OnMessage != null)
                OnMessage(sender, msg);
        }

        public  int ServerPort { get; set; }
    }
    public struct DataHolder
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long DateTime { get; set; }
        public bool Subscribed { get; set; }
        public int CurrentConnectCount { get; set; }
        public int ReConnectCount { get; set; }

        public bool CanReconnect
        {
            get
            {
                return CurrentConnectCount <= ReConnectCount;
            }
        }

    }
}
