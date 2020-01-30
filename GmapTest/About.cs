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
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            label1.Text = "Программное обеспечение \"Логистика торговых представителей\" предназначено для контроля и планирования работы торговых агентов. " +
                "По вопросам разработки и сотрудничества  пишите на e-mail: kolesnyk.ant@gmail.com \nРазработчик Колесник Антон";
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }
    }
}
