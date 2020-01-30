using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GmapTest
{
    public partial class Authorization : Form
    {
        public static string status = "none";
        public Authorization()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DBHandler db = new DBHandler();
            try
            {
                if (db.getAuth(textBox1.Text, textBox2.Text) != null)
                {
                    status = textBox1.Text;
                    Close();
                }
                else
                {
                    MessageBox.Show("Неправильный логин или пароль. Повторите ввод.", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox1.Text = "";
                    textBox2.Text = "";
                }
            }
            catch
            {
                MessageBox.Show("Проблемы при работе с базой данных. Обратитесь к администратору.", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
