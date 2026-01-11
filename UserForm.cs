using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Bee.Models;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Bee
{
    public partial class UserForm : Form
    {
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

        private int _userId;


        
        public UserForm(int userId)
        {
            InitializeComponent();
            _userId = userId;
            this.StartPosition = FormStartPosition.CenterParent;

            
            cmbRole.Items.AddRange(new object[] { "admin", "user" });

            if (_userId != 0)
            {
                this.Text = "แก้ไขข้อมูลผู้ใช้งาน";
                LoadUserData(_userId); // โหลดข้อมูลเดิม
            }
            else
            {
                this.Text = "เพิ่มผู้ใช้งานใหม่";
            }
        }

        private void UserForm_Load(object sender, EventArgs e)
        {

        }

        private void LoadUserData(int id)
        {
            string query = "SELECT username, email, role FROM users WHERE id = @Id";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                try
                {
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtUsername.Text = reader.GetString("username");
                            txtEmail.Text = reader.GetString("email");
                            cmbRole.SelectedItem = reader.GetString("role");
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("ข้อผิดพลาดในการโหลดข้อมูลผู้ใช้:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        

       

        // ======================================================
        // CRUD: SAVE (บันทึก/แก้ไข)
        // ======================================================

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string role = cmbRole.SelectedItem.ToString();
            string newPassword = txtNewPassword.Text;

            //  ประกาศตัวแปร hashedPassword ไว้ที่นี่ 
            string hashedPassword = null;

            // จัดการ Query และ Parameter
            string query;
            if (_userId == 0) // เพิ่มผู้ใช้ใหม่
            {
                // NOTE: การเพิ่มผู้ใช้ใหม่จะต้องมีรหัสผ่านเริ่มต้น
                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    MessageBox.Show("กรุณากำหนดรหัสผ่านเริ่มต้นสำหรับผู้ใช้ใหม่", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                //  กำหนดค่าให้กับ hashedPassword ที่ประกาศไว้
                hashedPassword = HashPassword(newPassword);

                // Query สำหรับ INSERT
                query = "INSERT INTO users (username, password, email, role) VALUES (@user, @pass, @email, @role)";
            }
            else // แก้ไขผู้ใช้เดิม
            {
                // เริ่มต้น Query สำหรับแก้ไขข้อมูลพื้นฐาน
                query = "UPDATE users SET username = @user, email = @email, role = @role";

                // หากมีการกรอกรหัสผ่านใหม่
                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    // 🔑 3. กำหนดค่าให้กับ hashedPassword ที่ประกาศไว้
                    hashedPassword = HashPassword(newPassword);
                    query += ", password = @pass"; // เพิ่มการอัปเดตรหัสผ่านใน Query
                }
                query += " WHERE id = @Id"; // จบ Query
            }

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@user", username);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@role", role);

                if (_userId != 0)
                {
                    command.Parameters.AddWithValue("@Id", _userId);
                }

                //  ตรวจสอบว่า hashedPassword มีค่าหรือไม่ ก่อนส่ง Parameter
                if (hashedPassword != null)
                {
                    command.Parameters.AddWithValue("@pass", hashedPassword);
                }

                try
                {
                    connection.Open();
                    if (command.ExecuteNonQuery() > 0)
                    {
                        
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    
                }
                catch (MySqlException )
                {
                    
                }
            }
        }

        // ======================================================
        // VALIDATION (การตรวจสอบความถูกต้อง)
        // ======================================================

        private bool IsValidEmail(string email)
        {
            // ใช้ Regex สำหรับรูปแบบอีเมลมาตรฐาน
            return System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtEmail.Text) || cmbRole.SelectedItem == null)
            {
                MessageBox.Show("กรุณากรอกชื่อผู้ใช้ อีเมล และกำหนดบทบาทให้ครบถ้วน", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("รูปแบบอีเมลไม่ถูกต้อง", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // ถ้าเป็นการเพิ่มผู้ใช้ใหม่ ต้องบังคับกรอกรหัสผ่าน
            if (_userId == 0 && string.IsNullOrWhiteSpace(txtNewPassword.Text))
            {
                MessageBox.Show("กรุณากำหนดรหัสผ่านเริ่มต้นสำหรับผู้ใช้ใหม่", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // NOTE: ถ้าคุณต้องการใช้ความซับซ้อนของรหัสผ่านเหมือนหน้า Register ให้เพิ่มโค้ดที่นี่

            return true;
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtEmail_TextChanged(object sender, EventArgs e)
        {

        }

        private void cmbRole_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtNewPassword_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel2_Click(object sender, EventArgs e)
        {

        }
    }
}
