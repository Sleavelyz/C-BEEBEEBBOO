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
    public partial class forgetpassword : Form
    {
        public forgetpassword()
        {
            InitializeComponent();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            // 1. สร้าง Instance ของ Form Login 
            // ตรวจสอบให้แน่ใจว่า Form Login ของคุณชื่อ Form1
            Form1 loginForm = new Form1();

            // 2. แสดง Form Login ขึ้นมา
            loginForm.Show();

            // 3. ปิด Form forgetpassword ปัจจุบัน (this) 
            // เมื่อผู้ใช้เปลี่ยนหน้าแล้ว Form ลืมรหัสผ่านนี้ก็ไม่จำเป็นต้องใช้อีก
            this.Close();
        }

        private void forgetpassword_Load(object sender, EventArgs e)
        {

        }
    }
}
