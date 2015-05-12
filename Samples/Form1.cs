using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Samples
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var manager = MessageManager.MessageManager.Instance;  //获取 MessageManager实例
            manager.UserName = "user1";  //设置登录用户名
            manager.UserPassword = "222";  //登录用户的密码
            manager.MessageServerHost = "10.80.5.222";  //消息服务的主机名
            manager.MessageServerPort = 5222;  //消息服务的端口
            manager.OnLogin += manager_OnLogin; // 注册登录事件,登陆成功后会回调
            manager.OnError += manager_OnError; //注册错误事件
            manager.OnMessage += manager_OnMessage;  //注册接收 message 事件
            manager.Start();   //启动服务
            button2.Enabled = true;  
        }

        void manager_OnMessage(FoxundermoonLib.XmppEx.Data.Message msg)
        {
            p("received a message");
            p(msg.ToJson());
            p("--------------------------------------------------------------------------");
            DataTable responceDt = msg.DataTable;

            foreach (var row in responceDt.Rows)
            {
                //do some thing
            }
        }

        void manager_OnError(XmppEx.ErrorEvent msg)
        {
            p("错误类型 :" + msg.ErrT.ToString() + "  错误消息:" + msg.EventData);

        }

        void manager_OnLogin(XmppEx.LoginEvent msg)
        {
            if (msg.Success)
            {
                p("登录成功");
            }
            else
            {
                p("登录失败  :"+msg.EventData);
            }
        }
        private void p(string msg)
        {
            this.SafeInvoke(() =>
            {
                textBox1.AppendText(msg + "\n");
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DataTable  dt = new DataTable();  //模拟数据表,使用的时候可以从数据库查询获得或者自己生成,用的内置 DataTable
            dt.Columns.Add("id");
            dt.Columns.Add("name");
            var row = dt.NewRow();
            row["id"]=1;
            row["name"] = "hello world";
            dt.Rows.Add(row);
            FoxundermoonLib.XmppEx.Data.Message m = new FoxundermoonLib.XmppEx.Data.Message();  //新建一条消息
            m.ToUser = "user2";    // 发送给谁?  ,不设置就发送给服务器.
            //m.FromUser = ""  //设置发送者,如果不设置,会用  UserName
            m.Command.Operation = "test";   //以下为 设置command ,可以间接操作客户端数据库
            m.Command.Name = "sendTask";  //....
            m.Command.Condition ="";
                m.Command.NeedResponse =true;
                m.Command.NeedBroadcast = true;
                                           //.....command 还有别的可以设置
            m.AddProperty("key", "some values ");   //发送字符信息给用户
            m.AddProperty("key2", "other values ");   //字符信息不限制数量,用不同的key
            m.setDataTable(dt);   //发送数据表给用户
            MessageManager.MessageManager.Instance.SendMessage(m); //发送
        }
    }
}
