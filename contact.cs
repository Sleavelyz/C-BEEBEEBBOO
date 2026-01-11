using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bee
{
    public partial class contact : Form
    {
        public contact()
        {
            InitializeComponent();
        }

        private void backbt_Click(object sender, EventArgs e)
        {
            //  สร้างอินสแตนซ์ใหม่ของฟอร์ม Login
            login loginForm = new login();

            //  แสดงฟอร์ม Login
            loginForm.Show();

            //  ปิดฟอร์มปัจจุบัน (เพื่อให้มันถูกล้างออกจากหน่วยความจำ)
            this.Close();
        }

        private void contact_Load(object sender, EventArgs e)
        {
            
        }
    }
}
