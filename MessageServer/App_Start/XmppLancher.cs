using System;
using System.Linq;
using MessageService.Core.Xmpp;
using MessageService.Core.Xmpp.Handler;
//using FoxundermoonLib.Diagnostics;
using System.Threading;

namespace MessageService
{
    public static class XmppLauncher
    {
        /// <summary>
        /// 注册 Xmpp相关配置
        /// </summary>
        public static void Launch()
        {
            Console.WriteLine("Starting xmpp service ......");
            //启动xmpp服务
            XmppServer.GetInstance().StartUp();
            Console.WriteLine("xmpp service started");
            //启动诊断
            //TraceManager.StartTrace();
        }
    }
}