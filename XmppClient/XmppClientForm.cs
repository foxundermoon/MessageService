using agsXMPP;
using FoxundermoonLib.EncryptUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FoxundermoonLib.Database;

namespace XmppClient
{
    public partial class XmppClientForm : Form
    {
        //aspDBManager dbm;
        public XmppClient xmppClient;
        public HttpClient httpClient;
        private static XmppClientForm ClientInstance;
        public bool AutoSend { get; set; }

        private static object objLock = new object();
        private Uinfo userInfo;
        public Uinfo UserInfo
        {
            get
            {
                return userInfo;
            }
            set
            {
                userInfo = value;
                xmppClient.Name = userInfo.Unumber;
                xmppClient.Password = userInfo.Passwd;
            }
        }
        private XmppClientForm()
        {
            InitializeComponent();
            initXmpp();
        }

        private void appendText(string text)
        {
            richTextBox1.AppendText(text+"\b");

        }
        public static XmppClientForm GetInstance()
        {
            lock (objLock)
            {
                if (ClientInstance == null)
                {
                    ClientInstance = new XmppClientForm();
                }
                return ClientInstance;
            }
        }
        private void initXmpp()
        {
            xmppClient = XmppClient.GetInstance();
            xmppClient.OnLogin += xmppClient_OnLogin;
            xmppClient.OnMessage += xmppClient_OnMessage;
            xmppClient.XmppErrorHandler += xmppClient_XmppErrorHandler;
            xmppClient.OnReloginFailed += xmppClient_OnReloginFailed;
            xmppClient.OnXmppError += xmppClient_OnXmppError;
        }

        void xmppClient_OnXmppError(object sender, Exception ex)
        {
            this.SafeInvoke(() =>
            {
                errorMsgLabel.Text = ex.Message + ex.ToString();
            });
        }
        void xmppClient_OnReloginFailed(object sender, ErrorMessage msg)
        {
            this.SafeInvoke(() =>
                {
                    MessageBox.Show("连接丢失 已超过重连次数....");
                });
        }
        public void Login()
        {
            xmppClient.Login();
            xmppClient.StartDaemon();
        }
        void xmppClient_XmppErrorHandler(object sender, ErrorMessage msg)
        {
            this.SafeInvoke(() =>
            {
                errorMsgLabel.Text = msg.ErrorMsg;

            });
        }
        void xmppClient_OnMessage(object sender, agsXMPP.protocol.client.Message msg)
        {
            //Invoke(new MethodInvoker(delegate()
            //{
            //    richTextBox1.AppendText(msg.From + "\n" + msg.Body);
            //}));
            string received =  EncryptUtil.DecryptBASE64ByGzip(msg.Body);
            string sql = string.Format(@"UPDATE [dbo].[record]  SET [status] ={0} where [guid]='{1}'",1,msg.Id);
            //dbm.ExecuteSql(sql);
            this.SafeInvoke(() =>
            {
                richTextBox1.AppendText(msg.From + "\n\r" + received +"\n");

            });
        }

        void xmppClient_OnLogin(object sender)
        {
            this.SafeInvoke(() =>
            {
                onlinestatus.Text = "online";
            });

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            datetimeLabel.Text = DateTime.Now.ToString() + "    ";
            toolTipLabel.Text = string.Empty;
        }

        private void XmppClientForm_Load(object sender, EventArgs e)
        {
            //dbm = new aspDBManager();
            string strConn = @"Data Source=10.80.5.222\sqlexpress;Persist Security Info=True;User ID=sa;Password=sql232381204;Database=xmppSend";
            //dbm.Open(strConn);
            //dbm.
            //datetimeLabel.Text = DateTime.Now.ToString();
            toolTipLabel.Text = string.Empty;
            if(AutoSend) {
                timer2.Enabled = true;

            }
        }
        public struct Uinfo
        {
            public string Unumber { get; set; }
            public string Passwd { get; set; }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var touser = textBox2.Text; ;
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                errorMsgLabel.Text = "请输入name";
            }
            else if (string.IsNullOrEmpty(textBox1.Text))
                {
                    errorMsgLabel.Text = "请输入发送的消息";
                }
                else 
                {
                    if (xmppClient.IsLongined)
                    {
                        string msgGuid = Guid.NewGuid().ToString().Replace("-","");
                        Jid to = new Jid(touser.ToString(), xmppClient.ServerJid.Server, string.Empty);
                        agsXMPP.protocol.client.Message msg = new agsXMPP.protocol.client.Message();
                        msg.From = xmppClient.LocalJid;
                        msg.To = new Jid(touser.ToString(), xmppClient.ServerJid.Server, string.Empty);
                        msg.Body = EncryptUtil.EncryptBASE64ByGzip( textBox1.Text);
                        msg.Language = "BASE64";
                        msg.Id = msgGuid;
                        xmppClient.XmppConnection.Send(msg);
                        appendText("send :" +textBox1.Text+"\n");
                        string sql = string.Format("INSERT INTO [dbo].[record] ([msg] ,[guid])  VALUES  ('{0}','{1}')",textBox1.Text,msgGuid);
                        //dbm.ExecuteSql(sql);
                    }
                    else
                    {
                        errorMsgLabel.Text = "未登录或者链接错误";
                    }
                }
               
            }
           

        private void XmppClientForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void timer2_Tick( object sender, EventArgs e ) {
            textBox2.Text="0";
            textBox1.Text = GeneratString(new Random().Next(5, 1000));
            button1.PerformClick();

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
                sb.Append(metaChar[ rd.Next(0,metaChar.Length)]);
            }
            return sb.ToString();
        }

    }
}
