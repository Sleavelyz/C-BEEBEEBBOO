using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Bee
{
    public partial class login : Form
    {
        private const string connectionString =
        "server=127.0.0.1;port=3306;database=bee;uid=root;password=;";

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
        public login()
        {
            InitializeComponent();
        }

        private void loginpanel(object sender, EventArgs e)
        {

        }

        private void backglogin_Paint(object sender, PaintEventArgs e)
        {

        }

        private void usernamelogin_TextChanged(object sender, EventArgs e)
        {

        }

        private void password_TextChanged(object sender, EventArgs e)
        {

        }

        private void loginbt_Click(object sender, EventArgs e)
        {
            //  ดึงค่าจาก TextBox
            string username = usernamelogin.Text;
            string password = this.password.Text;
            // 🔑 Hashing รหัสผ่านก่อนส่งไปตรวจสอบ
            string hashedPassword = HashPassword(password); // <--- Hash รหัสผ่านที่ผู้ใช้กรอก



            string query = "SELECT id, role FROM users WHERE username = @user AND password = @pass"; // โค้ดเดิมคือ SELECT role เดียว

            using(MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                // กำหนดค่าให้กับ Parameter ใน Query
                command.Parameters.AddWithValue("@user", username);
                command.Parameters.AddWithValue("@pass", hashedPassword); // <--- ต้องส่งค่าที่ Hash แล้ว

                try
                {
                    connection.Open();

                    // ใช้ MySqlDataReader เพื่ออ่านข้อมูล ***
                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.Read()) // ถ้าพบแถวข้อมูล (Login สำเร็จ)
                    {
                        // ดึงค่า role ออกมา
                        int userId = reader.GetInt32("id"); // ดึง User ID
                        string userRole = reader.GetString("role");
                        MessageBox.Show($"เข้าสู่ระบบสำเร็จ! (Role: {userRole})", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // กำหนดการนำทางตามบทบาท
                        this.Hide();

                        if (userRole == "admin")
                        {
                            // ไปยังหน้า Admin
                            admin adminForm = new admin(); 
                            adminForm.Show();
                        }
                        else // ผู้ใช้ทั่วไป (user) 
                        {
                            
                            

                            mainpage mainForm = new mainpage(userId); // *** แก้ไขตรงนี้ ***
                            mainForm.Show();
                        }
                    }
                    else
                    {
                        // Login ล้มเหลว
                        MessageBox.Show("ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("ข้อผิดพลาดของฐานข้อมูล:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void regisbt_Click(object sender, EventArgs e)
        {
            // สร้างอินสแตนซ์ของฟอร์ม Register
            register registrationForm = new register();

            // ซ่อนฟอร์ม Login ปัจจุบัน
            
            this.Hide();

         

            // 3. แสดงฟอร์ม Register
            registrationForm.Show();
        }

        private void forgetbt_Click(object sender, EventArgs e)
        {
            // สร้างอินสแตนซ์ของฟอร์ม Forget Password
            forgetpass recoveryForm = new forgetpass();

            //  ซ่อนฟอร์ม Login ปัจจุบัน
            this.Hide();

            // แสดงฟอร์ม Forget Password
            recoveryForm.Show();
        }

        private void contactbt_Click(object sender, EventArgs e)
        {
            // สร้างหน้า contact
            contact contactForm = new contact();

            
            contactForm.FormClosed += (s, args) => this.Show();

            // ซ่อนหน้า login แทนการปิด
            this.Hide();

            contactForm.Show();
        }
    }
}
