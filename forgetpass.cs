using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace Bee
{
    public partial class forgetpass : Form
    {
        private const string connectionString =
        "server=127.0.0.1;port=3306;database=bee;uid=root;password=;";
        public forgetpass()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ฟังก์ชันสำหรับแปลงรหัสผ่านเป็น SHA256 Hash
        /// </summary>
        
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void emailfg_TextChanged(object sender, EventArgs e)
        {

        }

        private void passwordfg_TextChanged(object sender, EventArgs e)
        {

        }

        private void backfg_Click(object sender, EventArgs e)
        {
            // สร้างอินสแตนซ์ใหม่ของฟอร์ม Login
            login loginForm = new login();

            //  แสดงฟอร์ม Login
            loginForm.Show();

            //  ปิดฟอร์ม Forget Password ปัจจุบัน
            this.Close();
        }

        private void confirmfg_Click(object sender, EventArgs e)
        {
            

            //  ดึงค่าจาก TextBox
            string email = emailfg.Text.Trim();          // อีเมลที่ผู้ใช้กรอก
            string newPassword = passwordfg.Text;    // รหัสผ่านใหม่ที่ผู้ใช้ต้องการ

            // ตรวจสอบข้อมูลเบื้องต้น
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("กรุณากรอกอีเมลและรหัสผ่านใหม่ให้ครบถ้วน", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string hashedPassword = HashPassword(newPassword);

            // คำสั่ง SQL UPDATE เพื่อเปลี่ยนรหัสผ่านตามอีเมลที่ระบุ
            string query = "UPDATE users SET password = @newpass WHERE email = @email";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                // กำหนดค่าให้กับ Parameter ใน Query
                command.Parameters.AddWithValue("@newpass", newPassword); // หรือ hashedPassword
                command.Parameters.AddWithValue("@email", email);

                try
                {
                    connection.Open();

                    
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        // **เปลี่ยนรหัสผ่านสำเร็จ**
                        MessageBox.Show("เปลี่ยนรหัสผ่านสำเร็จ! กรุณาเข้าสู่ระบบด้วยรหัสผ่านใหม่", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        //  กลับไปหน้า Login
                        login loginForm = new login();
                        loginForm.Show();
                        this.Close();
                    }
                    else
                    {
                        // ไม่พบผู้ใช้ด้วยอีเมลนี้
                        MessageBox.Show("ไม่พบผู้ใช้ด้วยอีเมลนี้ กรุณาตรวจสอบอีเมล", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("ข้อผิดพลาดของฐานข้อมูล:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void forgetpass_Load(object sender, EventArgs e)
        {

        }
    }
}
