using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XmppClient {
    static class Program {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main( string[] args ) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            LoginForm loginForm = new LoginForm();

            if(args.Length>0) {
                for(int i=0; i<args.Length; i++) {
                    switch(args[i]) {
                        case "-autologin":
                            if(args[i+1]=="false")
                                loginForm.AutoLogin =false;
                            else
                                loginForm.AutoLogin=true;
                            break;
                        case "-name":
                            loginForm.UserName=args[i+1];
                            break;
                        case "-password":
                            loginForm.Password=args[i+1];
                            break;
                        case "-autosend":
                            loginForm.AutoSend=true;
                            break;
                    }

                }

            }
            Application.Run(loginForm);
        }
    }
}
