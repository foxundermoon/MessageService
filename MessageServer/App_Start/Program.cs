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
            GpsDeviceServiceLauncher.Launch();
            Console.WriteLine("all done....");
       }
        public static void Exit()
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        public static void Exit(String msg)
        {
            Console.WriteLine(msg);
            Console.ReadKey();
            Exit();
        } 
    }
}
