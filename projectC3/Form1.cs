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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Chip1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Shapes1_Click(object sender, EventArgs e)
        {

        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel1_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel2_Click(object sender, EventArgs e)
        {
                    }

        private void guna2HtmlLabel3_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2TextBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {

        }

        private void guna2TextBox1_TextChanged_2(object sender, EventArgs e)
        {

        }

        private void guna2Button2_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2Button2_Click_2(object sender, EventArgs e)
        {
            // 1. สร้าง Instance ของ Form Register (FormRegister คือชื่อของหน้า Register)
            register registerForm = new register();

            // 2. แสดง Form Register ขึ้นมา
            registerForm.Show();

            // 3. ปิดหรือซ่อน Form Login (Form1)
            // * แนะนำให้ซ่อน Form Login ไว้ หากต้องการกลับมาหน้านี้อีกครั้ง
            this.Hide();

            // * หรือถ้ามั่นใจว่าจะไม่กลับมาหน้านี้แล้ว สามารถใช้ this.Close() ได้
        }

        private void guna2TextBox1_TextChanged_3(object sender, EventArgs e)
        {

        }

        private void guna2TextBox2_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel1_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            // 1. สร้าง Instance ของ Form Forget Password
            // 'forgetpassword' คือชื่อของ Form ที่คุณใช้สำหรับหน้านั้น
            forgetpassword forgotPasswordForm = new forgetpassword();

            // 2. แสดง Form Forget Password ขึ้นมา
            forgotPasswordForm.Show();

            // 3. ซ่อน Form Login ปัจจุบัน (Form1) ไว้ชั่วคราว
            // ผู้ใช้อาจจะต้องกลับมาหน้านี้หลังจากดำเนินการเสร็จ
            this.Hide();
        }
    }
}
