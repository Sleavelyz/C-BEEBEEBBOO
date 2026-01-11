using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bee
{
    public partial class HistoryUC : UserControl
    {
        private const string connectionString =
            "server=127.0.0.1;port=3306;database=bee;uid=root;password=;";
        public HistoryUC()
        {
            InitializeComponent();
            InitializeComboBoxes();
            // โหลดข้อมูลเริ่มต้นเมื่อ UC โหลด (เช่น ปีปัจจุบัน)
            LoadMonthlySales(DateTime.Now.Month, DateTime.Now.Year);
            LoadYearlySales(DateTime.Now.Year);
        }

        // ฟังก์ชันสำหรับใส่ค่าเริ่มต้นใน ComboBoxes
        private void InitializeComboBoxes()
        {
            // เติมเดือน (1 ถึง 12)
            for (int i = 1; i <= 12; i++)
            {
                cmbMonth.Items.Add(i.ToString("00")); // เช่น "01", "02"
            }
            cmbMonth.SelectedIndex = DateTime.Now.Month - 1; // เลือกเดือนปัจจุบัน

            // เติมปี (สมมติ 5 ปีล่าสุด)
            int currentYear = DateTime.Now.Year;
            for (int i = 0; i < 5; i++)
            {
                cmbYearMonthly.Items.Add((currentYear - i).ToString());
                cmbYearly.Items.Add((currentYear - i).ToString());
            }
            cmbYearMonthly.SelectedIndex = 0;
            cmbYearly.SelectedIndex = 0;
        }

        // =================================================================
        // B. โหลดตารางยอดขายรายเดือน
        // =================================================================
        private void LoadMonthlySales(int month, int year)
        {
            string query = @"
        SELECT 
            DATE(OrderDate) AS OrderDay, 
            SUM(TotalAmount) AS DailyTotal
        FROM orders
        WHERE MONTH(OrderDate) = @month AND YEAR(OrderDate) = @year
        GROUP BY OrderDay
        ORDER BY OrderDay;
    ";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@month", month);
                command.Parameters.AddWithValue("@year", year);

                try
                {
                    connection.Open();
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // แสดงข้อมูลในตาราง
                    dgvMonthlySales.DataSource = dt;

                    FormatGridViewColumns();

                    // ****************************************************
                    // เพิ่มโค้ด: คำนวณและแสดงยอดรวมรายเดือน
                    // ****************************************************
                    decimal totalSales = 0;
                    if (dt.Rows.Count > 0)
                    {
                        // คำนวณผลรวมของคอลัมน์ 'DailyTotal' ทั้งหมด
                        totalSales = dt.AsEnumerable().Sum(row => row.Field<decimal>("DailyTotal"));
                    }

                    // แสดงผลรวมใน TextBox/Label
                    txtMonthlyTotal.Text = totalSales.ToString("N2") + " บาท"; // N2: แสดงทศนิยม 2 ตำแหน่ง
                                                                               // ****************************************************

                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("ข้อผิดพลาดในการดึงยอดขายรายเดือน: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // =================================================================
        // C. โหลดตารางยอดขายรายปี
        // =================================================================
        private void LoadYearlySales(int year)
        {
            string query = @"
        SELECT 
            MONTH(OrderDate) AS OrderMonth,
            SUM(TotalAmount) AS MonthlyTotal
        FROM orders
        WHERE YEAR(OrderDate) = @year
        GROUP BY OrderMonth
        ORDER BY OrderMonth;
    ";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@year", year);

                try
                {
                    connection.Open();
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // แสดงข้อมูลในตารางรายปี
                    dgvYearlySales.DataSource = dt;

                    FormatGridViewColumns();

                    // ****************************************************
                    // เพิ่มโค้ด: คำนวณและแสดงยอดรวมรายปี
                    // ****************************************************
                    decimal totalSales = 0;
                    if (dt.Rows.Count > 0)
                    {
                        // คำนวณผลรวมของคอลัมน์ 'MonthlyTotal' ทั้งหมด
                        totalSales = dt.AsEnumerable().Sum(row => row.Field<decimal>("MonthlyTotal"));
                    }

                    // แสดงผลรวมใน TextBox/Label
                    txtYearlyTotal.Text = totalSales.ToString("N2") + " บาท";
                    // ****************************************************
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("ข้อผิดพลาดในการดึงยอดขายรายปี: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void FormatGridViewColumns()
        {
            // กำหนดรูปแบบสำหรับตารางยอดขายรายเดือน (dgvMonthlySales)
            if (dgvMonthlySales.Columns.Contains("DailyTotal"))
            {
                // กำหนดรูปแบบตัวเลข N2 (แสดงคอมมาหลักพัน และทศนิยม 2 ตำแหน่ง)
                dgvMonthlySales.Columns["DailyTotal"].DefaultCellStyle.Format = "N2";
                dgvMonthlySales.Columns["DailyTotal"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            // กำหนดรูปแบบสำหรับตารางยอดขายรายปี (dgvYearlySales)
            if (dgvYearlySales.Columns.Contains("MonthlyTotal"))
            {
                // กำหนดรูปแบบตัวเลข N2 (แสดงคอมมาหลักพัน และทศนิยม 2 ตำแหน่ง)
                dgvYearlySales.Columns["MonthlyTotal"].DefaultCellStyle.Format = "N2";
                dgvYearlySales.Columns["MonthlyTotal"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }


        private void HistoryUC_Load(object sender, EventArgs e)
        {

        }

        private void cmbMonth_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cmbYearMonthly_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnFilterMonthly_Click(object sender, EventArgs e)
        {
            if (cmbMonth.SelectedItem != null && cmbYearMonthly.SelectedItem != null)
            {
                int month = Convert.ToInt32(cmbMonth.SelectedItem.ToString());
                int year = Convert.ToInt32(cmbYearMonthly.SelectedItem.ToString());
                LoadMonthlySales(month, year);
            }
        }

        private void dgvMonthlySales_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnFilterYearly_Click(object sender, EventArgs e)
        {
            if (cmbYearly.SelectedItem != null)
            {
                int year = Convert.ToInt32(cmbYearly.SelectedItem.ToString());
                LoadYearlySales(year);
            }
        }

        private void cmbYearly_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dgvYearlySales_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void txtMonthlyTotal_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtYearlyTotal_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
