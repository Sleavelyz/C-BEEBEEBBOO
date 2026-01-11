using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Bee
{
    public partial class UCManageOrders : UserControl
    {
        private const string connectionString = "server=127.0.0.1;port=3306;database=bee;uid=root;password=;";
        public UCManageOrders()
        {
            InitializeComponent();

            //  ผูก Logic การจัดการทั้งหมด
            LoadOrdersData();
            dgvOrders.CellContentClick += dgvOrders_CellContentClick;
            dgvOrders.CellValueChanged += dgvOrders_CellValueChanged;
            dgvOrders.EditMode = DataGridViewEditMode.EditOnEnter;
        }

        private void LoadOrdersData()
        {
            // ดึงข้อมูลคำสั่งซื้อทั้งหมด (รวมชื่อผู้ใช้และ Path สลิป)
            string query = @"
                SELECT 
                    o.OrderId, 
                    u.username AS 'ผู้สั่งซื้อ', 
                    o.OrderDate AS 'วันที่สั่งซื้อ', 
                    o.TotalAmount AS 'ยอดรวม',
                    o.ShippingAddress AS 'ที่อยู่',
                    o.PaymentStatus AS 'สถานะชำระเงิน',
                    o.SlipImagePath  
                FROM orders o
                JOIN users u ON o.UserId = u.id
                ORDER BY o.OrderDate DESC";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    //  ตั้งค่าโครงสร้างคอลัมน์ก่อนผูกข้อมูล
                    SetupOrderGridView();

                    //  ผูกข้อมูล (ไม่มีการสร้างคอลัมน์ซ้ำซ้อนแล้ว)
                    dgvOrders.DataSource = dt;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("ข้อผิดพลาดในการโหลดคำสั่งซื้อ:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SetupOrderGridView()
        {
            dgvOrders.AutoGenerateColumns = false;
            dgvOrders.Columns.Clear(); // ล้างคอลัมน์เดิมที่อาจถูกสร้างไว้

            // =========================================================
            //  กำหนดคอลัมน์ข้อมูล (ReadOnly)
            // =========================================================

            // OrderId (ซ่อน)
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn() { Name = "OrderId", HeaderText = "ID", DataPropertyName = "OrderId", Visible = false });

            // ข้อมูลหลัก (ReadOnly = true)
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "ผู้สั่งซื้อ", DataPropertyName = "ผู้สั่งซื้อ", ReadOnly = true });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "วันที่สั่งซื้อ", DataPropertyName = "วันที่สั่งซื้อ", ReadOnly = true });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "ยอดรวม", DataPropertyName = "ยอดรวม", ReadOnly = true });

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn()
            {
                HeaderText = "",
                DataPropertyName = "ยอดรวม",
                ReadOnly = true,
                // กำหนดรูปแบบตัวเลข N2 (คอมมาหลักพัน, ทศนิยม 2 ตำแหน่ง)
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            // ที่อยู่ 
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn() { Name = "AddressCol", HeaderText = "ที่อยู่", DataPropertyName = "ที่อยู่", ReadOnly = true, Width = 100 });

           

            //  คอลัมน์ Path สลิป (ซ่อน)
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn() { Name = "SlipImagePathCol", HeaderText = "SlipImagePath", DataPropertyName = "SlipImagePath", Visible = false, ReadOnly = true });

            //  ปุ่ม 'ดูสลิป'
            DataGridViewButtonColumn slipBtn = new DataGridViewButtonColumn();
            slipBtn.Name = "ViewSlipBtn";
            slipBtn.HeaderText = "สลิป";
            slipBtn.Text = "ดูสลิป";
            slipBtn.UseColumnTextForButtonValue = true;
            dgvOrders.Columns.Add(slipBtn);

            //  ComboBox สถานะ (แก้ไขได้)
            DataGridViewComboBoxColumn statusCombo = new DataGridViewComboBoxColumn();
            statusCombo.Name = "PaymentStatusCol";
            statusCombo.HeaderText = "สถานะชำระเงิน";
            statusCombo.DataPropertyName = "สถานะชำระเงิน";
            statusCombo.Items.AddRange("Pending Verification", "Verified", "Rejected");
            dgvOrders.Columns.Add(statusCombo);

            // ปรับสไตล์ Header
            dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            dgvOrders.ColumnHeadersHeight = 40;
            dgvOrders.EnableHeadersVisualStyles = false;
        }

        private void dgvOrders_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            

            //  Logic ปุ่ม "ดูสลิป" (ViewSlipBtn)
            if (dgvOrders.Columns[e.ColumnIndex].Name == "ViewSlipBtn")
            {
                // การเข้าถึงค่า Slip Path โดยใช้ Name ของคอลัมน์ที่ซ่อน: SlipImagePathCol
                string slipPath = dgvOrders.Rows[e.RowIndex].Cells["SlipImagePathCol"].Value?.ToString();

                if (!string.IsNullOrEmpty(slipPath))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(slipPath); // เปิดไฟล์
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"ไม่สามารถเปิดไฟล์สลิปได้: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("ไม่พบสลิปโอนเงินสำหรับคำสั่งซื้อนี้", "ข้อมูลไม่ครบ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void dgvOrders_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // ตรวจสอบเฉพาะคอลัมน์ ComboBox และแถวข้อมูลจริง
            if (e.RowIndex >= 0 && dgvOrders.Columns[e.ColumnIndex].Name == "PaymentStatusCol")
            {
                if (dgvOrders.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                {
                    //  ดึง OrderId 
                    int orderId = (int)dgvOrders.Rows[e.RowIndex].Cells["OrderId"].Value;
                    string newStatus = dgvOrders.Rows[e.RowIndex].Cells["PaymentStatusCol"].Value.ToString();

                    // 2. รันคำสั่ง UPDATE ในฐานข้อมูล
                    UpdatePaymentStatusInDB(orderId, newStatus);
                }
            }
        }

        /// <summary>
        /// เมธอดสำหรับบันทึกสถานะการชำระเงินกลับไปยังตาราง orders
        /// </summary>
        private void UpdatePaymentStatusInDB(int orderId, string newStatus)
        {
            string query = "UPDATE orders SET PaymentStatus = @Status WHERE OrderId = @Id";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Status", newStatus);
                command.Parameters.AddWithValue("@Id", orderId);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("ข้อผิดพลาดในการบันทึกสถานะชำระเงิน:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UCManageOrders_Load(object sender, EventArgs e)
        {

        }
    }
}
