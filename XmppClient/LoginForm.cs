using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XmppClient {
    public partial class LoginForm : Form {
        XmppClientForm client;
        public bool AutoLogin { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool AutoSend { get; set; }
        public LoginForm( ) {
            InitializeComponent();
        }
        private void button2_Click( object sender, EventArgs e ) {
            this.Close();
            System.Environment.Exit(0);
        }
        private void button1_Click( object sender, EventArgs e ) {
            if(string.IsNullOrWhiteSpace(textBox2.Text)) {
                label4.ForeColor = Color.Red;
                label4.Text = "请输入密码";
            } else if(string.IsNullOrWhiteSpace(textBox2.Text)) {
                label3.ForeColor = Color.Red;
                label3.Text ="请输入用户名";
            }else{
                login();
            }
        }

        private void login( ) {
            button1.Enabled = false;
            XmppClientForm.Uinfo user = new XmppClientForm.Uinfo();
            user.Unumber = textBox1.Text;
            user.Passwd = textBox2.Text;
            client = XmppClientForm.GetInstance();
            client.Text = ">>>" + textBox1.Text + "<<<" + client.Text;
            client.UserInfo = user;
            client.AutoSend = AutoSend;
            client.Login();
            client.xmppClient.OnLogin += new agsXMPP.ObjectHandler(onlogin);
            toolStripStatusLabel1.Text = "登录中,请稍等....";
            timer1.Enabled = true;
        }

        private void onlogin( object sender ) {
            OnLogined logined = delegate( ) {
                try {
                    client.Show();
                    //this.Close();
                    this.Hide();
                } catch(Exception we) {
                    string msg = we.Message;
                }
            };
            try {
                if(this.InvokeRequired) {
                    this.Invoke(logined);
                } else {
                    logined();
                }
            } catch(Exception ignore) {
                string msg = ignore.Message;
            }

        }

        private void timer1_Tick( object sender, EventArgs e ) {
            button1.Enabled = true;
            toolStripStatusLabel1.Text = "登录失败";
            timer1.Enabled = false;

        }
        private delegate void OnLogined( );

        private void LoginForm_Load( object sender, EventArgs e ) {
            if(AutoLogin) {
                textBox1.Text = UserName;
                textBox2.Text = Password;
                login();
            }
        }

    }
}
