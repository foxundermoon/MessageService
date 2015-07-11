using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FoxundermoonLib.Database.Mysql;

namespace DbOpTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            var sb = new StringBuilder();
            sb.Append("Server=").Append(textBox1.Text).Append("; ")
                .Append("Database=").Append(textBox2.Text).Append("; ")
                .Append("User=").Append(textBox3.Text).Append("; ")
                .Append("Password=").Append(textBox4.Text).Append("; ")
                .Append("Charset=").Append("utf8").Append("; ");
            //.Append("Pooling=").Append(ConfigurationManager.AppSettings["MysqlPooling"]).Append("; ")
            //.Append("Max Pool Size=").Append(ConfigurationManager.AppSettings["MysqlMaxPoolSize"]).Append("; ")
            //.Append("Use Procedure Bodies=").Append(ConfigurationManager.AppSettings["MysqlUseProcedureBodies"]).Append("; ")
            //.Append("Allow Zero Datetime=").Append("true").Append("; ");
            //return sb.ToString();


            MysqlHelper.connectionString = sb.ToString();



            enableAllButton();
        }

        private void enableAllButton()
        {
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
        }

        string tbname;
        Random r = new Random();
        private void button2_Click(object sender, EventArgs e1)
        {
            string tb = "testTable" + r.Next(100, 1000);
            tbname = tb;
            string sql = "DROP TABLE IF EXISTS `" + tb + "`;"
            + "CREATE TABLE `" + tb + "` (  `id` int(11) NOT NULL AUTO_INCREMENT,  `data` varchar(255) DEFAULT NULL,  PRIMARY KEY (`id`)) ENGINE=InnoDB DEFAULT CHARSET=utf8; ";
            try
            {

                MysqlHelper.ExecuteNonQuery(sql);
                p("创建了  " + tb);
                p("请检查数据库是否创建成功");
            }
            catch (Exception e)
            {
                p(e.Message);
                p(e.StackTrace);
            }

        }

        void p(string msg)
        {
            textBox5.AppendText(msg + "\r\n");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {

            string insert = "insert" + r.Next(8000);
            string sql = string.Format("INSERT INTO `{0}` (`data`) VALUES ('{1}')", tbname, insert);
            int id = MysqlHelper.ExecuteNonQuery(sql);
            p("插入了" + id + "条数据   data 列为" + insert + "  请检查表");
            }
            catch (Exception ew)
            {
                p(ew.Message);
                p(ew.StackTrace);
            }

        }
    }
}
