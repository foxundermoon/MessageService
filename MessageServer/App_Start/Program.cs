using System;
//using System.Collections.Generic;
using MessageService.Core.Xmpp;
using FoxundermoonLib.Database.Mysql;
using MessageService.App_Start.Tools;
using MessageService.App_Start;
namespace MessageService {
  public  class Program {
        [STAThread]
       public static void Main( string[] args ) {
           //AlterTables.AddOneCoulmn();
           //AlterTables.Change版本列();
           //AlterTables.Change是否历史();
           XmppLauncher.Launch();
            //XmppServer.GetInstance().StartUp();
            HttpApiLauncher.Launch();
            //SocketServiceLauncher.Launch();
            //SocketServiceLauncher.Bootstrap();
            var superSocketServer= Core.SocketMessage.Inspection.Detector.Instance;
            superSocketServer.Interval = 10;
            superSocketServer.Start();
            Console.WriteLine("all done....");
       }
        public static void Exit()
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        public static void Exit(String msg)
        {
            Console.WriteLine(msg);
            Console.WriteLine("请按任意键退出！");
            Console.ReadKey();
            Exit();
        } 
    }
}
