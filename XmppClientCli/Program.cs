using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using XmppClient;
using MongoDB.Bson;
using MongoDB.Driver;
using agsXMPP;
using System.Diagnostics;
using agsXMPP.protocol.client;
using FoxundermoonLib.EncryptUtil;
using System.Configuration;

namespace XmppClientCli {
    class Program {
        static string xmppServer = ConfigurationManager.AppSettings["xmppserver"].ToString();
        //static IMongoCollection<BsonDocument> collection;
        //static IMongoCollection<BsonDocument> errorColection;
        Random rand = new Random();
        Timer autoSending;
        XmppClient.XmppClient xmppClient;
        static void Main( string[] args ) {
            if(args!=null && args.Length>=1) {
                ClientProfi(args);
            } else {
               
            }
            ClientMutiSend();
            Console.ReadKey();
        }

        private static void ClientMutiSend( ) {
            Program p = new Program();
            var xmppClient = XmppClient.XmppClient.Instance;
            xmppClient.Password = "123456";
            xmppClient.ServerJid = new Jid("80000@"+xmppServer+"/server");
            xmppClient.LocalJid = new Jid("0@"+xmppServer+"/XmppCli");
            p.xmppClient = xmppClient;
            xmppClient.Login();
            p.regEvent();

           
        }

    

        private static Message CreatRandomMsg( ) {
            Message msg = new Message();
            msg.To = new Jid("0@10.80.5.222/client");
            msg.From = new Jid("80000@10.80.5.222/server");
            msg.Id=Guid.NewGuid().ToString().Replace("-","");
            msg.Body = GeneratString(80000);
            return msg;

        }

        private static void ClientProfi( string[] args ) {
            int from=-1;
            int to=-1;
            int threadPerProcess =100;
            var name="";
            var password ="";
            var mutithread = false;
            var mutiprocess = false;
            var single = false;
            if(args==null || args.Length <1) {
                showHelp();
            } else if(args.Contains("-name") && args.Contains("-password")) {

                foreach(var arg in args) {
                    if(arg.ToLower().Contains("mutithread"))
                        mutithread=true;
                    if(arg.ToLower().Contains("mutiprocess"))
                        mutiprocess = true;
                    if(arg.ToLower().Contains("single"))
                        single=true;

                }
                for(var i=0; i<args.Length; i++) {
                    if(args[i].ToLower().Contains("name")) {
                        name=args[i+1];
                    }
                    if(args[i].ToLower().Contains("password")) {
                        password = args[i+1];
                    }
                    if(args[i].ToLower().Contains("threadnumber"))
                        int.TryParse(args[i+1], out threadPerProcess);
                }
                if(!single) {
                    if(name !=null && name.Contains(":")) {
                        var names = name.Split(':');
                        from = int.Parse(names[0]);
                        to = int.Parse(names[1]);
                    }
                }
                //var mongoServer = ConfigurationManager.AppSettings["MongoServer"].ToString();
                //var mongClient = new MongoClient("mongodb://10.80.5.222:27017");
                //var mongClient = new MongoClient(mongoServer);
                //var database =  mongClient.GetDatabase("XmppClient");
                //collection =database.GetCollection<BsonDocument>("Sent");
                //errorColection =database.GetCollection<BsonDocument>("XmppError");
                if(single) {
                    L("single client longin-> user:"+name +"  password:"+password);
                    SingleLogin(name, password);
                } else if(mutiprocess && !mutithread) {
                    L("mutiprocess login");
                    MutiProcessLogin(from, to, password);
                } else if(mutithread && !mutiprocess) {
                    L("muti thread longin");
                    MutiThreadLogin(from, to, password);
                } else if(mutiprocess && mutiprocess) {
                    L("muti process and thread longin");
                    MutiProcessAndThread(from, to, password, threadPerProcess);
                    //L("must just onlyone mutiThread or mutiProcess ");
                }

            } else {
                showHelp();
            }
            Console.ReadLine();
        }


        void regEvent( ) {
            xmppClient.XmppConnection.OnLogin +=XmppConnection_OnLogin;
            xmppClient.XmppConnection.OnMessage +=XmppConnection_OnMessage;
            xmppClient.XmppConnection.OnError +=XmppConnection_OnError;
            //xmppClient.OnLogin+=xmppClient_OnLogin;
        }

        void xmppClient_OnLogin( object sender ) {
            try {

            for(var i=1; 1<100000; i++) {
                xmppClient.XmppConnection.Send(CreatRandomMsg());
                Console.WriteLine(i+ "sended");
                Thread.Sleep(100);
            }
            } catch(Exception e) {
                Console.WriteLine(e.Message);
            }

        }

        void XmppConnection_OnError( object sender, Exception ex ) {
            L("xmpp error:"+ ex.Message);
            //errorColection.InsertOneAsync(new BsonDocument { 
            //    {"Message",ex.Message},
            //    {"Data",new BsonDocument( ex.Data)},
            //    {"HelpLink",ex.HelpLink},
            //});
        }

        void XmppConnection_OnMessage( object sender, agsXMPP.protocol.client.Message msg ) {
            L(xmppClient.Name + "received msg  content:"+msg.Body.Substring(0, msg.Body.Length>20?20:msg.Body.Length));
        }
        void XmppConnection_OnLogin( object sender ) {
            L(xmppClient.Name + "login success");
            autoSending = new Timer(( state ) => {
                Message msg = new Message();
                msg.To = xmppClient.ServerJid;
                msg.From = xmppClient.LocalJid;
                string sentCon = GeneratString(rand.Next(5, 5000));
                msg.Body = EncryptUtil.EncryptBASE64ByGzip(sentCon);
                msg.Id= Guid.NewGuid().ToString().Replace("-", "");
                xmppClient.XmppConnection.Send(msg);
                L(xmppClient.Name +" send a message to server, length:"+sentCon.Length);
                //collection.InsertOneAsync(new BsonDocument {
                //    {"Content",sentCon},
                //    {"from",xmppClient.Uid},
                //    {"to",xmppClient.ServerJid.User},
                //    {"stime",new BsonDateTime(DateTime.UtcNow)}
                //});
            }, null, 0, 1000);
        }

        private static void MutiProcessAndThread( int from, int to, string password,int threadPerProcess ) {
            int total = to-from;
            if(total<5)
                L("the user  you give is too small,should >5");
            else {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName ="XmppClientCli.exe";
                if(total<threadPerProcess) {
                    info.Arguments = "--mutithread -name "+from+":"+to+" -password 123456 --mu_tiprocess";
                    L(info.Arguments);
                    Process.Start(info);
                } else {
                    double result = (Convert.ToDouble(total) / Convert.ToDouble(threadPerProcess));
                    var slice =Convert.ToInt32( Math.Ceiling(result));
                    for(var i=1;i<=slice;i++){
                        info.Arguments = "--mutithread -name "
                            +(from+(i-1)*threadPerProcess)+":"+
                            ((from+i*threadPerProcess)>to? to: (from+i*threadPerProcess))
                            +" -password 123456";
                        L(info.Arguments);
                        Process.Start(info);
                        L("sleep wait .......");
                        Thread.Sleep(TimeSpan.FromSeconds(3));
                        //Thread.Sleep(threadPerProcess * 100);

                    }

                }
      
            }
        }

        private static void MutiProcessLogin( int from, int to, string password ) {
            for(var i=from; i<to; i++) {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "XmppClientCli.exe";
                info.Arguments = "--single -name "+i+" -password 123456 --mu_tiprocess";
                //var cmd = "XmppClientCli -name " +i+" -password "+password +" --single";
                //var cmd = Process.GetCurrentProcess().ProcessName + " -name " +i+" -password "+password +" --single";
                System.Diagnostics.Process.Start(info);
                L("为user "+i+" 开启了一个进程....");
                Thread.Sleep(200);


            }
        }

        private static void MutiThreadLogin( int from, int to, string password ) {
            for(var i = from; i<to; i++) {
                //new Thread(( ) => {
                //    var xmppClient = XmppClient.XmppClient.Instance;
                //    xmppClient.Password = password;
                //    xmppClient.ServerJid = new Jid("0@10.80.5.222/server");
                //    xmppClient.LocalJid = new Jid(i+"@10.80.5.222/XmppCli");
                //    xmppClient.OnLogin+=xmppClient_OnLogin;

                //}
                //    ).Start();
                Program p =new Program();
                //var xmppClient = XmppClient.XmppClient.Instance;
                var xmppClient = XmppClient.XmppClient.CreatNewInstance();
                xmppClient.Password = password;
                xmppClient.Name = Convert.ToString(i);
                xmppClient.ServerJid = new Jid("0@"+xmppServer+"/server");
                xmppClient.LocalJid = new Jid(i+"@"+xmppServer+"/XmppCli");
                p.xmppClient = xmppClient;
                //p.regEvent();
                new Thread(p.run).Start();
                L("为user "+i+" 开启了一个线程....");
                Thread.Sleep(100);

            }
        }
        private static void SingleLogin( string name, string password ) {
            Program p = new Program();
            var xmppClient = XmppClient.XmppClient.Instance;
            xmppClient.Password = password;
            xmppClient.ServerJid = new Jid("0@"+ xmppServer +"/server");
            xmppClient.LocalJid = new Jid(name+"@"+xmppServer+"/XmppCli");
            xmppClient.Name = name;
            p.xmppClient = xmppClient;
            //p.regEvent();
            L(name +" start login...");
            p.run();

        }
        void run( ) {
            L(xmppClient.Name +" start login ");
            xmppClient.Login();
            regEvent();


        }

        private static void showHelp( ) {
            L("请输入参数");
            L("-name <from:to>  eg:   -name 1:500");
            L("-password <password>  eg  -password 123456");
            L("--mutithread  ");
            L("--mutiprocess   ");
            L("--single ");
        }
        static void L( String msg ) {
            Console.WriteLine(msg);
        }
        public static string GeneratString( int p ) {
            char[] metaChar = @"引证解释
\依照情势；顺应时机。
\《陈书·徐世谱传》：“世谱性机巧，谙解旧法，所造器械，竝随机损益，妙思出人。” 宋陈亮《酌古论·崔浩》：“而不知事固有随机立权者，乌可\以琐琐顾虑哉！”鲁迅《集外集拾遗补编·“骗月亮”》：“他们只想到将来会碰到月亮，放鞭炮去声援，却没有想到也会碰到天狗。并且不知道即\使现在并不声援，将来万一碰到月亮时，也可以随机说出一番道理来敷衍过去的。”[1] 
\2基本含义
\编辑
\
\因素
\客观世界是运动的，运动是有规律的。物质运动的规律可以分为必然规律和统计规律。必然规律是指事物本质的规律，它毫无例外地适用于事物所有个\体；统计规律是指通过对随机现象的大量观察，所呈现出来的事物的集体性规律。统计规律与事物的单一个体的性质时而偶合，时而近似，时而简直\没有什么联系。
\客观世界作用于事物各个个体的因素分为基本因素和次要因素两类，基本因素决定事物的必然规律，次要因素使事物呈现统计规律。人们所能认识而且\\能够控制的因素是基本因素，而大量的次要因素未能为人们所认识或未能被人们所控制，但只要存在次要因素的影响，就必然会有所表现。比如发射\炮弹，其基本因素也 是人们所能控制的是它的初始条件--初速、发射角等，这些可以通过弹道方程（必然规律）计算出炮弹的落地点，但炮弹在飞行\过程中会受到空气的阻力--风速、风向、空气的湿度、温度等的影响，它们使得炮弹不能落在它的准确的目的地。
\科学研究的目的，就是要发现反映事物本质的客观规律，即排除偶然性的掩盖与干扰，为此必须首先认识偶然性。于是统计学应运而生，统计学不是直\接研究事物本质的必然规律，而是通过随机现象来发现事物的统计规律，并把它应用于对客观规律的认识和把握。
\基本现象
\可以做实验表明随机。
\把一个硬币扔到天空，谁也不知道它落下来时是正面还是反面.这种现象叫做随机。
\[英译:random]
\随机现象是概率论研究的主要对象，随机现象的背后往往存在着深刻的规律在内。
\从数学的角度来研究社会和自然现象可以把这些现象分为以下三类：
\确定现象
\事前可预言的现象，即在准确地重复某些条件下，它的结果总是肯定的。如：在一个标准大气压下给水加热到100℃便会沸腾。比如质量守恒定律、牛顿\定律反映就是这类现象。研究这类现象的数学工具有数学分析、几何、代数、微分方程等。
\随机现象
\
\事前不可预言的现象，即在相同条件下重复进行试验，每次结果未必相同，或知道事物过去的状况，但未来的发展却不能完全肯定。如：以同样的方式\抛置硬币却可能出现正面向上也可能出现反面向上；走到某十字路口时，可能正好是红灯，也可能正好是绿灯。研究这类现象的数学工具是概率论和\统计。
\模糊现象

\事物本身的含义不确定的现象。如：“情绪稳定”与“情绪不稳定”，“健康”与“不健康”，“年青”与“年老”。研究这类现象的数学工具是模糊\数学。
\确定性现象与随机现象的共同特点是事物本身的含义确定；随机现象与模糊现象的共同特点是不确定性，随机现象中是指事件的结果不确定，而模糊现\象中是指事物本身的定义不确定。概率论与统计学将数学的应用从必然现象扩大到随机现象的领域，模糊数学则将数学的应用范围从清晰确定扩大到\模糊现象的领域。".ToCharArray();
            Random rd = new Random();
            StringBuilder sb = new StringBuilder();
            for(int i=0; i<p; i++) {
                sb.Append(metaChar[rd.Next(0, metaChar.Length)]);
            }
            return sb.ToString();
        }

    }
}
