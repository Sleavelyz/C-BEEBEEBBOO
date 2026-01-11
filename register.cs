using Guna.UI2.WinForms;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Security.Cryptography; 
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Bee
{
    public partial class register : Form
    {
        private bool IsValidPassword(string password)
        {
            // ตัวอย่าง: รหัสผ่านต้องมีความยาวอย่างน้อย 8 ตัวอักษร 
            // และอย่างน้อยต้องประกอบด้วยตัวอักษรและตัวเลข

            //  ต้องมีตัวพิมพ์ใหญ่, ตัวพิมพ์เล็ก, ตัวเลข, และความยาวขั้นต่ำ
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$";

            return Regex.IsMatch(password, pattern);
        }
        // ฟังก์ชันตรวจสอบ Username: ต้องเป็น a-z, A-Z, 0-9 เท่านั้น
        private bool IsValidUsername(string username)
        {
            // ตรวจสอบความยาว: 3-20 ตัวอักษร
            if (username.Length < 3 || username.Length > 20)
            {
                return false;
            }
            // ใช้ Regex: ^[a-zA-Z0-9]+$ คือต้องประกอบด้วยตัวอักษรหรือตัวเลขเท่านั้น
            return Regex.IsMatch(username, @"^[a-zA-Z0-9]+$");
        }

        // ฟังก์ชันตรวจสอบ Email: ต้องเป็นรูปแบบ email มาตรฐาน
        private bool IsValidEmail(string email)
        {
            // ใช้ Regex สำหรับรูปแบบอีเมลมาตรฐาน
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
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

        private const string connectionString =
    "server=127.0.0.1;port=3306;database=bee;uid=root;password=;";
        public register()
        {
            InitializeComponent();
        }

        private void usernameregis_TextChanged(object sender, EventArgs e)
        {

        }

        private void emailregis_TextChanged(object sender, EventArgs e)
        {

        }

        private void regisbt2_Click(object sender, EventArgs e)
        {
            // ***************************************************************
            //  ดึงค่าจากคุณสมบัติ .Text ของ Control โดยตรง
            // ***************************************************************
            string username = usernameregis.Text;
            string password = passwordregis.Text;
            string email = emailregis.Text;

            // ตรวจสอบ Username
            if (!IsValidUsername(username))
            {
                MessageBox.Show("ชื่อผู้ใช้ไม่ถูกต้อง! ต้องเป็นตัวอักษร a-z และตัวเลข 0-9 เท่านั้น (3-20 ตัวอักษร)", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // หยุดการทำงานถ้าไม่ผ่าน
            }

            // ตรวจสอบ Email
            if (!IsValidEmail(email))
            {
                MessageBox.Show("รูปแบบอีเมลไม่ถูกต้อง กรุณากรอกอีเมลที่ใช้งานได้", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // หยุดการทำงานถ้าไม่ผ่าน
            }

            // ตรวจสอบ Password (เพิ่มขั้นต่ำเพื่อความปลอดภัย)
            if (password.Length < 8)
            {
                MessageBox.Show("รหัสผ่านต้องมีความยาวอย่างน้อย 8 ตัวอักษร", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // หยุดการทำงานถ้าไม่ผ่าน
            }
            // ***************************************************************
            if (!IsValidPassword(password))
            {
                MessageBox.Show("รหัสผ่านไม่ปลอดภัย! ต้องมีความยาวอย่างน้อย 8 ตัวอักษร และประกอบด้วย ตัวพิมพ์ใหญ่ (A-Z), ตัวพิมพ์เล็ก (a-z) และตัวเลข (0-9)", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // หยุดการทำงานถ้าไม่ผ่าน
            }

            //  เข้ารหัสรหัสผ่านก่อนบันทึก!
            string hashedPassword = HashPassword(password);

            //  เพิ่มคอลัมน์ 'role' และกำหนดค่าเริ่มต้นเป็น 'user' ให้ผู้ใช้ใหม่
            string query = "INSERT INTO users (username, password, email, role) VALUES (@user, @pass, @email, 'user')";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                // กำหนดค่าให้กับ Parameter ใน Query
                command.Parameters.AddWithValue("@user", username);
                command.Parameters.AddWithValue("@pass", hashedPassword); // ✅ แก้ไข: ใช้ค่าที่ถูก Hash แล้ว
                command.Parameters.AddWithValue("@email", email);

                try
                {
                    connection.Open();

                    //  ใช้ ExecuteNonQuery
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        // **สมัครสมาชิกสำเร็จ**
                        MessageBox.Show("สมัครสมาชิกสำเร็จ! กรุณาเข้าสู่ระบบ", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        //  กลับไปหน้า Login
                        login loginForm = new login();
                        loginForm.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("ไม่สามารถสมัครสมาชิกได้ กรุณาลองใหม่อีกครั้ง", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (MySqlException ex)
                {
                    // จับข้อผิดพลาด Duplicate entry
                    if (ex.Number == 1062)
                    {
                        MessageBox.Show("ชื่อผู้ใช้นี้ถูกใช้ไปแล้ว กรุณาเลือกชื่ออื่น", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("ข้อผิดพลาดของฐานข้อมูล:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void backbt_Click(object sender, EventArgs e)
        {
            // 1. สร้างอินสแตนซ์ใหม่ของฟอร์ม Login
            login loginForm = new login();

            // 2. แสดงฟอร์ม Login
            loginForm.Show();

            // 3. ปิดฟอร์ม Register ปัจจุบัน
            this.Close();
        }

        private void passwordregis_TextChanged(object sender, EventArgs e)
        {

        }

        private void regispanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void register_Load(object sender, EventArgs e)
        {

        }
    }
}
