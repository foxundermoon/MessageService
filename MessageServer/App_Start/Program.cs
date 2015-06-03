using System;
//using System.Collections.Generic;
using MessageService.Core.Xmpp;
using FoxundermoonLib.Database.Mysql;
using MessageService.App_Start.Tools;
namespace MessageService {
  public  class Program {
        [STAThread]
       public static void Main( string[] args ) {
           //AlterTables.AddOneCoulmn();
            XmppLauncher.Launch();
            //XmppServer.GetInstance().StartUp();
            //HttpApiLauncher.Launch();
            Console.WriteLine("all done....");
       }
    }
}
