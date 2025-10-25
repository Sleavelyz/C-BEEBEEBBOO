using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace projectC3
{
    public partial class register : Form
    {
        public register()
        {
            InitializeComponent();
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            // 1. สร้าง Instance ของ Form Login (Form1 คือชื่อของหน้า Login)
            // ตรวจสอบให้แน่ใจว่า Form Login ของคุณชื่อ Form1
            Form1 loginForm = new Form1();

            // 2. แสดง Form Login ขึ้นมา
            loginForm.Show();

            // 3. ปิด Form Register ปัจจุบัน (this)
            // การใช้ Close() จะลบ Form register ออกจากหน่วยความจำ
            this.Close();
        }
    }
}
