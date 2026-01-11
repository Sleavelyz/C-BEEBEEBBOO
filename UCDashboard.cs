using Guna.UI2.WinForms;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bee
{
    public partial class UCDashboard : UserControl
    {
        private const string connectionString =
            "server=127.0.0.1;port=3306;database=bee;uid=root;password=;";

        public UCDashboard()
        {
            InitializeComponent();
            // เรียกใช้ฟังก์ชันหลักเมื่อ Control เริ่มต้น

            this.lblFilteredSales.Click += lblFilteredSales_Click;
            LoadDashboardMetrics();
            LoadTopSellingProducts();
            LoadLowStockProducts();
        }

        private void UCDashboard_Load(object sender, EventArgs e)
        {
            // สามารถย้ายการเรียกฟังก์ชันมาไว้ใน Load Event ได้
        }

        // ======================================================
        // METRICS (ยอดขายรวม, จำนวนออเดอร์, จำนวนสินค้า)
        // ======================================================

        private void LoadDashboardMetrics()
        {

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Query 1: ยอดขายทั้งหมด (Total Sales) - ใช้ตาราง orders และ order_details
                    string salesQuery = @"
                        SELECT IFNULL(SUM(od.Quantity * od.Price), 0) 
                        FROM order_details od
                        JOIN orders o ON od.OrderId = o.OrderId
                        WHERE o.Status != 'Cancelled'";

                    decimal totalSales = Convert.ToDecimal(new MySqlCommand(salesQuery, connection).ExecuteScalar());
                    // NOTE: ตรวจสอบให้แน่ใจว่า lblTotalSales ถูกตั้งชื่อถูกต้อง
                    lblTotalSales.Text = "฿" + totalSales.ToString("#,##0.00");

                    // Query 2: จำนวนออเดอร์ทั้งหมด (Total Orders)
                    string ordersQuery = "SELECT COUNT(OrderId) FROM orders";
                    int totalOrders = Convert.ToInt32(new MySqlCommand(ordersQuery, connection).ExecuteScalar());
                    lblTotalOrders.Text = totalOrders.ToString();

                    // Query 3: สินค้าทั้งหมด (Total Products)
                    string productsQuery = "SELECT COUNT(ProductId) FROM products";
                    int totalProducts = Convert.ToInt32(new MySqlCommand(productsQuery, connection).ExecuteScalar());
                    lblTotalProducts.Text = totalProducts.ToString();

                    // Query 4: สินค้าใกล้หมด (Low Stock Count)
                    string lowStockQuery = "SELECT COUNT(ProductId) FROM products WHERE StockQuantity <= LowStockThreshold";
                    int lowStockCount = Convert.ToInt32(new MySqlCommand(lowStockQuery, connection).ExecuteScalar());
                    lblLowStockCount.Text = lowStockCount.ToString();
                }
                catch (MySqlException ex)
                {
                    // Error นี้จะเกิดขึ้นถ้าตาราง orders, order_details ยังไม่ได้ถูกสร้าง
                    MessageBox.Show("ข้อผิดพลาดในการดึงข้อมูล Dashboard (โปรดตรวจสอบตาราง orders, order_details):\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ======================================================
        //  สินค้าขายดี 3 อันดับแรก (Top Selling Products)
        // ======================================================

        private void LoadTopSellingProducts()
        {         
           
            string query = @"
        SELECT  
            p.Name AS 'ชื่อสินค้า',  
            SUM(od.Quantity) AS 'ยอดขาย',  
            SUM(od.Quantity * od.Price) AS 'รายได้'
        FROM order_details od
        JOIN orders o ON od.OrderId = o.OrderId
        JOIN products p ON od.ProductId = p.ProductId
        GROUP BY p.ProductId, p.Name
        ORDER BY ยอดขาย DESC
        LIMIT 3";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                //  สร้าง MySqlCommand และส่งต่อ
                MySqlCommand command = new MySqlCommand(query, connection);

                //  เรียกใช้ Helper Method ที่รับ MySqlCommand
                LoadDataToDataGridView(command, dgvTopSelling);
            }
        }

        // ======================================================
        // สินค้าที่ต้องเติม (Low Stock Products)
        // ======================================================

        private void LoadLowStockProducts()
        {
            string query = @"
        SELECT 
            Name AS 'ชื่อสินค้า', 
            StockQuantity AS 'คงเหลือ', 
            LowStockThreshold AS 'ต่ำสุด'
        FROM products 
        WHERE StockQuantity <= LowStockThreshold
        ORDER BY StockQuantity ASC";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                // สร้าง MySqlCommand
                MySqlCommand command = new MySqlCommand(query, connection);
                LoadDataToDataGridView(command, dgvLowStock); // 🔑 ส่ง MySqlCommand ไป
            }
        }

        // ======================================================
        // Helper Method: สำหรับดึงข้อมูลเข้า DataGridView
        // ======================================================

        private void FormatGridViewColumns()
        {
            // จัดรูปแบบตารางสินค้าขายดี (dgvTopSelling)
            // คอลัมน์ 'รายได้' ควรเป็นตัวเลขที่มีทศนิยม 2 ตำแหน่งและคอมมาหลักพัน
            if (dgvTopSelling.Columns.Contains("รายได้"))
            {
                dgvTopSelling.Columns["รายได้"].DefaultCellStyle.Format = "#,##0.00";
                dgvTopSelling.Columns["รายได้"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            // จัดรูปแบบตาราง Low Stock (dgvLowStock)
            // คอลัมน์ 'คงเหลือ' และ 'ต่ำสุด' ควรเป็นจำนวนเต็ม
            if (dgvLowStock.Columns.Contains("คงเหลือ"))
            {
                dgvLowStock.Columns["คงเหลือ"].DefaultCellStyle.Format = "N0";
            }
            if (dgvLowStock.Columns.Contains("ต่ำสุด"))
            {
                dgvLowStock.Columns["ต่ำสุด"].DefaultCellStyle.Format = "N0";
            }

            // จัดรูปแบบตาราง Daily Transactions (dgvDailyTransactions)
            if (dgvDailyTransactions.Columns.Contains("ยอดเงิน"))
            {
                dgvDailyTransactions.Columns["ยอดเงิน"].DefaultCellStyle.Format = "#,##0.00";
            }
        }

        private void LoadDataToDataGridView(MySqlCommand command, DataGridView dgv)
        {
            using (DataTable dt = new DataTable())
            {
                try
                {
                    //  เปิด Connection ที่อยู่ใน Command
                    command.Connection.Open();

                    MySqlDataAdapter da = new MySqlDataAdapter(command);
                    da.Fill(dt);

                    dgv.DataSource = dt;
                    dgv.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                    FormatGridViewColumns();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error loading dashboard grid: " + ex.Message);
                }
                finally
                {
                    //  ปิด Connection เสมอ
                    if (command.Connection.State == ConnectionState.Open)
                    {
                        command.Connection.Close();
                    }
                }
            }
        }

        private void lblTotalSales_Click(object sender, EventArgs e)
        {

        }

        private void lblTotalOrders_Click(object sender, EventArgs e)
        {

        }

        private void lblTotalProducts_Click(object sender, EventArgs e)
        {

        }

        private void lblLowStockCount_Click(object sender, EventArgs e)
        {

        }

        private void dgvTopSelling_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvLowStock_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void UCDashboard_Load_1(object sender, EventArgs e)
        {

        }

        private void dtpDateFilter_ValueChanged(object sender, EventArgs e)
        {

        }

        private void btnFilterDate_Click(object sender, EventArgs e)
        {
            // เกดปุ่ม ให้โหลดข้อมูลใหม่ทั้งหมดโดยใช้ Logic การกรองวันที่
            LoadDashboardMetrics();
            LoadTopSellingProducts();
            //  เพิ่มการเรียกใช้สำหรับตารางใหม่
            LoadDailyTransactions();

            UpdateFilteredSalesTotal();
        }

        private void dgvDailyTransactions_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        /// <summary>
        /// โหลดรายการคำสั่งซื้อทั้งหมดในวันที่ถูกกรอง (ไม่จำกัดจำนวน)
        /// </summary>
        private void LoadDailyTransactions()
        {
            string selectedDate = dtpDateFilter.Value.ToString("yyyy-MM-dd");

            //  Query : ดึงรายละเอียดสินค้า 
            string query = @"
        SELECT  
            o.OrderId AS 'OrderId',
            u.username AS 'ลูกค้า',
            p.Name AS 'ชื่อสินค้า',
            od.Quantity AS 'จำนวน',
            (od.Quantity * od.Price) AS 'ยอดเงิน'  -- ยอดรวมของรายการสินค้านั้น
        FROM orders o
        JOIN users u ON o.UserId = u.id
        JOIN order_details od ON o.OrderId = od.OrderId
        JOIN products p ON od.ProductId = p.ProductId
        WHERE DATE(o.OrderDate) = @SelectedDate
        ORDER BY o.OrderId DESC";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@SelectedDate", selectedDate);

                //  เรียก Helper Method เพื่อโหลดข้อมูลเข้าตารางใหม่
                LoadDataToDataGridView(command, dgvDailyTransactions); // dgvDailyTransactions คือตารางใหม่
            }
        }

        private void lblFilteredSales_Click(object sender, EventArgs e)
        {
            UpdateFilteredSalesTotal();


        }

        private void UpdateFilteredSalesTotal()
        {
            DateTime selectedDate = dtpDateFilter.Value.Date;

            string query = @"
        SELECT IFNULL(SUM(od.Quantity * od.Price), 0)
        FROM order_details od
        JOIN orders o ON od.OrderId = o.OrderId
        WHERE DATE(o.OrderDate) = @d
          AND (o.Status IS NULL OR o.Status <> 'Cancelled');";

            using (var conn = new MySqlConnection(connectionString))
            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.Add("@d", MySqlDbType.Date).Value = selectedDate;

                try
                {
                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    decimal total = (result == null || result == DBNull.Value) ? 0m : Convert.ToDecimal(result);

                    // อัปเดตคอนโทรลที่เห็นในหน้าจอจริง ๆ
                    lblFilteredSales.Text = "฿" + total.ToString("#,##0.00");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("UpdateFilteredSalesTotal error: " + ex.Message);
                    lblFilteredSales.Text = "฿0.00";
                }
            }
        }
    }
}
