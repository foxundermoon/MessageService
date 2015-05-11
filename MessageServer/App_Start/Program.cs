using System;
//using System.Collections.Generic;
using MessageService.Core.Xmpp;

namespace MessageService {
  public  class Program {
        [STAThread]
       public static void Main( string[] args ) {
            XmppLauncher.Launch();
            //XmppServer.GetInstance().StartUp();
        }
    }
}
