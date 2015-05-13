using FoxundermoonLib.XmppEx.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmppEx;

namespace MessageManager
{
  public  class MessageManager
    {
        public XmppEx.XmppClient XmppClient { get; set; }
        static MessageManager instance;
        public static MessageManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (instance == null)
                        instance = new MessageManager();
                    return instance;
                }
            }
        }
        static object _lock = new object();
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string MessageServerHost { get; set; }
        public int MessageServerPort { get; set; }
        public event LoginHandler OnLogin;
        public event MessageHandle OnMessage;
        public event ErrorHandler OnError;


        public bool Start()
        {

            if( string.IsNullOrEmpty(UserName))
            {
                OnError(new ErrorEvent("用户名为空"));
                return false;

            }
            if (string.IsNullOrEmpty(UserPassword))
            {
                OnError(new ErrorEvent("用户名密码为空"));
                return false;
            }
            if (string.IsNullOrEmpty(MessageServerHost))
            {
                OnError(new ErrorEvent("消息服务主机名为空"));
                return false;
            }
            if (MessageServerPort < 1)
            {
                OnError(new ErrorEvent("消息服务端口号没设置"));
                return false;
            }



            XmppClient = XmppEx.XmppClient.GetInstance();
            XmppClient.Name = UserName;
            XmppClient.Password = UserPassword;
            var server = new agsXMPP.Jid("0@" +MessageServerHost  +"/WinForm");
            XmppClient.ServerJid = server;
            XmppClient.LocalJid = new agsXMPP.Jid(UserName+"@" + MessageServerHost + "/WinForm ");
            regXmppEvent();
            try
            {
                XmppClient.Login();
                return true;

            }
            catch (Exception e)
            {
                OnError(new ErrorEvent("启动xmpp客户端错误," + e.Message));
                return false;
            }

        }


        public bool Stop()
        {
            unRegEvent();
            XmppClient.XmppConnection.Close();
            return true;
        }
        public void Restart()
        {
            Stop();
            Start();
        }

        private void unRegEvent()
        {
            XmppClient.OnLogin -= XmppClient_OnLogin;
            XmppClient.OnMessage -= XmppClient_OnMessage;
            XmppClient.OnXmppError -= XmppClient_OnXmppError;
        }
        private void regXmppEvent()
        {
            XmppClient.OnLogin += XmppClient_OnLogin;
            XmppClient.OnMessage += XmppClient_OnMessage;
            XmppClient.OnXmppError += XmppClient_OnXmppError;
        }

        void XmppClient_OnXmppError(ErrorEvent msg)
        {
            OnError(msg);
        }


        void XmppClient_OnLogin(object sender)
        {
            OnLogin(new LoginEvent(true));
        }

        void XmppClient_OnMessage(object sender, agsXMPP.protocol.client.Message msg)
        {
            var body = "";
            var subject = "";
            if (msg != null)
            {
                if ("BASE64".Equals(msg.Language))
                {
                    body = FoxundermoonLib.Encrypt.EncryptUtil.DecryptBASE64ByGzip(msg.Body);
                }else{
                    body = msg.Body;
                }
                subject = msg.Subject;
                Message msgEntity= new Message();
                try
                {
                    msgEntity.SetJsonMessage(body);
                    msgEntity.SetJsonCommand(subject);
                    OnMessage(msgEntity);
                }
                catch (Exception e)
                {
                    var err = new ErrorEvent(e.Message);
                    err.ErrT = ErrorEvent.ErrorType.ParseMessageFailed;
                    OnError(err);
                    throw e;
                }
             
            }
        }




        public void SendMessage(Message m)
        {
            agsXMPP.protocol.client.Message xmppMsg = new agsXMPP.protocol.client.Message();
            xmppMsg.Language = "BASE64";
            xmppMsg.Subject = m.GetJsonCommand();
            xmppMsg.Body = FoxundermoonLib.Encrypt.EncryptUtil.EncryptBASE64ByGzip(m.ToJson());
            if (string.IsNullOrEmpty(m.FromUser))
                m.FromUser = UserName;
            if (string.IsNullOrEmpty(m.ToUser))
                m.ToUser = "0";

            xmppMsg.From = new agsXMPP.Jid(m.FromUser + "@" + MessageServerHost +":" +MessageServerPort);
            xmppMsg.To = new agsXMPP.Jid(m.ToUser + "@" + MessageServerHost + ":" + MessageServerPort);
            XmppClient.XmppConnection.Send(xmppMsg);
        }
    }
}
