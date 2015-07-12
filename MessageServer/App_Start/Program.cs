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
           XmppLauncher.Launch();
            //XmppServer.GetInstance().StartUp();
            //HttpApiLauncher.Launch();
            //SocketServiceLauncher.Launch();
           //SocketServiceLauncher.Bootstrap();
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
