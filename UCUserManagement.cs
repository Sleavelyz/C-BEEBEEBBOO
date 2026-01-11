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
using System.Windows.Forms;


namespace Bee
{
    public partial class UCUserManagement : UserControl
    {
        private const string connectionString =
            "server=127.0.0.1;port=3306;database=bee;uid=root;password=;";
        public UCUserManagement()
        {
            InitializeComponent();
            // เมื่อ User Control ถูกสร้าง ให้โหลดข้อมูลผู้ใช้ทันที
            LoadUsersToDataGridView();
        }

        private void UCUserManagement_Load(object sender, EventArgs e)
        {

        }

        // ======================================================
        //  (การดึงข้อมูลและแสดง)
        // ======================================================

        public void LoadUsersToDataGridView()
        {
            List<User> users = new List<User>();
            // Query ยังคงเดิม
            string query = "SELECT id, username, email, role FROM users";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                try
                {
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                id = reader.GetInt32("id"),
                                username = reader.GetString("username"),
                                email = reader.GetString("email"),
                                role = reader.GetString("role")
                            });
                        }
                    }

                    // ล้างคอลัมน์เก่าและผูกข้อมูลใหม่
                    dataGridViewUsers.Columns.Clear();
                    dataGridViewUsers.AutoGenerateColumns = false;
                    dataGridViewUsers.DataSource = users;

                    //  เพิ่มคอลัมน์ข้อมูล
                    dataGridViewUsers.Columns.Add("UsernameCol", "ชื่อผู้ใช้");
                    dataGridViewUsers.Columns["UsernameCol"].DataPropertyName = "username";

                    dataGridViewUsers.Columns.Add("EmailCol", "อีเมล");
                    dataGridViewUsers.Columns["EmailCol"].DataPropertyName = "email";

                    dataGridViewUsers.Columns.Add("RoleCol", "บทบาท");
                    dataGridViewUsers.Columns["RoleCol"].DataPropertyName = "role";

                    //  เพิ่มคอลัมน์ปุ่มแก้ไข (Edit Button)
                    DataGridViewButtonColumn editButtonCol = new DataGridViewButtonColumn();
                    editButtonCol.Name = "EditButton"; 
                    editButtonCol.HeaderText = "แก้ไข";
                    editButtonCol.Text = "✏️ แก้ไข";
                    editButtonCol.UseColumnTextForButtonValue = true;
                    editButtonCol.Width = 80;
                    dataGridViewUsers.Columns.Add(editButtonCol);

                    //  เพิ่มคอลัมน์ปุ่มลบ (Delete Button)
                    DataGridViewButtonColumn deleteButtonCol = new DataGridViewButtonColumn();
                    deleteButtonCol.Name = "DeleteButton"; 
                    deleteButtonCol.HeaderText = "ลบ";
                    deleteButtonCol.Text = "🗑️ ลบ";
                    deleteButtonCol.UseColumnTextForButtonValue = true;
                    deleteButtonCol.Width = 80;
                    dataGridViewUsers.Columns.Add(deleteButtonCol);

                    //  เพิ่มคอลัมน์ ID (ซ่อน) เพื่อใช้ดึงค่าเมื่อคลิก
                    DataGridViewTextBoxColumn idCol = new DataGridViewTextBoxColumn();
                    idCol.Name = "id";
                    idCol.DataPropertyName = "id";
                    idCol.Visible = false;
                    dataGridViewUsers.Columns.Add(idCol);

                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("ข้อผิดพลาดในการดึงข้อมูลผู้ใช้งาน:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ======================================================
        //  DELETE (การลบผู้ใช้งาน)
        // ======================================================

        private void DeleteUser(int userId)
        {
            var confirmResult = MessageBox.Show("คุณแน่ใจหรือไม่ว่าต้องการลบบัญชีนี้?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmResult == DialogResult.Yes)
            {
                string query = "DELETE FROM users WHERE id = @Id";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", userId);
                    try
                    {
                        connection.Open();
                        if (command.ExecuteNonQuery() > 0)
                        {
                            MessageBox.Show("ลบบัญชีผู้ใช้สำเร็จ", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadUsersToDataGridView(); // รีโหลดข้อมูลหลังลบ
                        }
                        else
                        {
                            MessageBox.Show("ไม่พบบัญชีผู้ใช้", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show("ข้อผิดพลาดของฐานข้อมูล:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // ======================================================
        //  การจัดการปุ่มใน User Control
        // ======================================================

       

        private void dataGridViewUsers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
            if (e.RowIndex >= 0)
            {
                int userId = (int)dataGridViewUsers.Rows[e.RowIndex].Cells["id"].Value;

               
                if (dataGridViewUsers.Columns[e.ColumnIndex].Name == "EditButton")
                {
                    ShowUserForm(userId); // แก้ไขผู้ใช้งาน
                }


                if (dataGridViewUsers.Columns[e.ColumnIndex].Name == "DeleteButton")
                {
                    DeleteUser(userId); // ลบผู้ใช้งาน
                }
            }
        }

        /// <summary>
        /// เปิดฟอร์ม Popup เพิ่ม/แก้ไขผู้ใช้งาน
        /// </summary>
        private void ShowUserForm(int userId)
        {
           
            using (UserForm form = new UserForm(userId))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadUsersToDataGridView(); // รีโหลดข้อมูลเมื่อบันทึกสำเร็จ
                }
            }
        }

        private void btnAddUser_Click_1(object sender, EventArgs e)
        {
           
            ShowUserForm(0); 
        }
    }
}
