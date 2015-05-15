using agsXMPP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Core.Xmpp
{
    public partial class XmppServer
    {
        public event UserOnlineStatusHandler UserOffLineHandler;
        public event UserOnlineStatusHandler UserOnLineHandler;
        public void UserOffline(string name)
        {
            XmppSeverConnection _ = null;
            XmppConnectionDic.TryRemove(name, out  _);
            var offLine = new FoxundermoonLib.XmppEx.Data.Message();
            offLine.Command.Name = FoxundermoonLib.XmppEx.Command.Cmd.UserOffLine;
            offLine.AddProperty("UserName", name);
            Broadcast(offLine);
            if (UserOffLineHandler != null)
                UserOffLineHandler(name);
        }

        public void UserOnline(string name)
        {
            var onLogin = new FoxundermoonLib.XmppEx.Data.Message();
            onLogin.Command.Name = FoxundermoonLib.XmppEx.Command.Cmd.UserLogin;
            onLogin.AddProperty("UserName", name);
            Broadcast(onLogin);
            if (UserOnLineHandler != null)
                UserOnLineHandler(name);
        }
    }
}
