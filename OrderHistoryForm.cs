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

namespace Bee
{
    public partial class OrderHistoryForm : Form
    {
        // Connection String (สามารถคัดลอกมาจากฟอร์มอื่นได้)
        private const string connectionString =
            "server=127.0.0.1;port=3306;database=bee;uid=root;password=;";

        private int currentUserId;
        public OrderHistoryForm(int userId)
        {
            InitializeComponent();
            currentUserId = userId;
            this.Text = "Order History for User ID: " + currentUserId;

            LoadHistory(currentUserId);
        }
        public OrderHistoryForm()
        {
            InitializeComponent();
        }

        private void LoadHistory(int userId)
        {
            // คำสั่ง SQL: ดึงข้อมูลคำสั่งซื้อทั้งหมดที่ตรงกับ User ID
            string query = @"
        SELECT 
            OrderID, 
            OrderDate, 
            TotalAmount, 
            status 
        FROM orders 
        WHERE UserID = @userId  -- ใช้ชื่อคอลัมน์ UserID ตามที่เราแก้ไขไป
        ORDER BY OrderDate DESC"; // เรียงลำดับจากล่าสุดไปเก่าสุด

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@userId", userId);

                try
                {
                    connection.Open();
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // สมมติชื่อ DataGridView ของคุณคือ dgvHistory
                    dgvHistory.DataSource = dt;

                    // ****************************************************
                    // เพิ่มโค้ด: คำนวณและแสดงผลสรุป
                    // ****************************************************

                    // 1. คำนวณจำนวนรายการสั่งซื้อทั้งหมด
                    int totalOrders = dt.Rows.Count;

                    // 2. คำนวณยอดเงินรวมทั้งหมด
                    decimal totalAmountSum = 0;
                    if (totalOrders > 0)
                    {
                        // ใช้ LINQ เพื่อรวมค่าจากคอลัมน์ TotalAmount
                        totalAmountSum = dt.AsEnumerable().Sum(row => row.Field<decimal>("TotalAmount"));
                    }

                    // 3. แสดงผลใน Control
                    txtTotalOrders.Text = $"{totalOrders} รายการ";
                    txtTotalAmount.Text = $"{totalAmountSum:N2} บาท"; // N2: แสดงทศนิยม 2 ตำแหน่ง

                    // ****************************************************

                    if (totalOrders == 0)
                    {
                        MessageBox.Show("ไม่พบประวัติการสั่งซื้อสำหรับผู้ใช้นี้", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("ข้อผิดพลาดในการดึงประวัติการสั่งซื้อ: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void OrderHistoryForm_Load(object sender, EventArgs e)
        {

        }

        private void dgvHistory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void txtTotalOrders_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtTotalAmount_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel2_Click(object sender, EventArgs e)
        {

        }
    }
}
