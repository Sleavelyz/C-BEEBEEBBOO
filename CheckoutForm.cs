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
    public partial class CheckoutForm : Form
    {
        private const string connectionString = "server=127.0.0.1;port=3306;database=bee;uid=root;password=;"; 
        // ใช้ CartItem จาก mainpage.cs
        private List<mainpage.CartItem> _cartItems;
        private int _userId;

        //  เพิ่ม Public Properties สำหรับส่งค่ากลับไป mainpage 
        public string ShippingAddress { get; set; }
        public string PaymentSlipPath { get; set; }
        public CheckoutForm(List<mainpage.CartItem> cartItems, int userId)
        {
            InitializeComponent();
            _cartItems = cartItems;
            _userId = userId;
            this.StartPosition = FormStartPosition.CenterScreen;
            DisplayCartSummary();
        }

        private void CheckoutForm_Load(object sender, EventArgs e)
        {

        }

        private void DisplayCartSummary()
        {
            //  ตั้งค่า DataGridView
            dgvSummary.DataSource = null;
            dgvSummary.AutoGenerateColumns = false;
            dgvSummary.Columns.Clear();

            // ตั้งค่าความสูงของ Header (แก้ปัญหา Header ถูกตัด)
            dgvSummary.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvSummary.ColumnHeadersHeight = 35;

            // สีพื้นหลังส่วนหัว: RGB(249, 165, 0)
            dgvSummary.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(249, 165, 0);

            // สีตัวอักษร: สีดำ
            dgvSummary.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;

            // ต้องตั้งค่าเป็น False เพื่อให้ใช้สี RGB Custom ได้
            dgvSummary.EnableHeadersVisualStyles = false;
            dgvSummary.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);

            // เพิ่มคอลัมน์ (ใช้ DataGridViewButtonColumn)

            // คอลัมน์ที่ 1: ชื่อสินค้า (อ่านอย่างเดียว)
            dgvSummary.Columns.Add(new DataGridViewTextBoxColumn() { Name = "NameCol", HeaderText = "สินค้า", DataPropertyName = "Name", ReadOnly = true, Width = 150 });

            //  คอลัมน์ที่ 2: ปุ่มลดจำนวน (-)
            dgvSummary.Columns.Add(new DataGridViewButtonColumn() { Name = "MinusCol", HeaderText = "-", Text = "—", UseColumnTextForButtonValue = true, Width = 40 });

            //  คอลัมน์ที่ 3: จำนวนสินค้า (อ่านอย่างเดียว)
            dgvSummary.Columns.Add(new DataGridViewTextBoxColumn() { Name = "QuantityCol", HeaderText = "จำนวน", DataPropertyName = "Quantity", ReadOnly = true, Width = 60 });

            //  คอลัมน์ที่ 4: ปุ่มเพิ่มจำนวน (+)
            dgvSummary.Columns.Add(new DataGridViewButtonColumn() { Name = "PlusCol", HeaderText = "+", Text = "+", UseColumnTextForButtonValue = true, Width = 40 });

            // คอลัมน์ที่ 5: ราคาต่อชิ้น (อ่านอย่างเดียว)
            dgvSummary.Columns.Add(new DataGridViewTextBoxColumn() { Name = "PriceCol", HeaderText = "ราคา/ชิ้น", DataPropertyName = "Price", ReadOnly = true, Width = 80 });

            // คอลัมน์ที่ 6: ราคารวม (อ่านอย่างเดียว)
            dgvSummary.Columns.Add(new DataGridViewTextBoxColumn() { Name = "TotalCol", HeaderText = "รวม", DataPropertyName = "Total", ReadOnly = true, Width = 80 });


            //  ผูกรายการสินค้า
            dgvSummary.DataSource = _cartItems; // ผูกกับ List<CartItem> โดยตรง

            //  สรุปยอดรวมเริ่มต้น
            UpdateTotalSummary();
        }

        private void dgvSummary_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // ตรวจสอบว่าคลิกที่ Cell ที่ถูกต้องและเป็นแถวข้อมูลจริง
            if (e.RowIndex < 0 || e.RowIndex >= _cartItems.Count || dgvSummary.Columns[e.ColumnIndex].GetType() != typeof(DataGridViewButtonColumn))
            {
                return;
            }
            
            bool updateNeeded = false; 
            // ดึงรายการ CartItem ที่ถูกคลิก
            var item = _cartItems[e.RowIndex];
            int maxStock = GetMaxStockQuantity(item.ProductId);
            //  คลิกปุ่มเพิ่ม (+)
            if (dgvSummary.Columns[e.ColumnIndex].Name == "PlusCol")
            {
                // ตรวจสอบ: จำนวนปัจจุบัน + 1 ต้องไม่เกินสต็อกสูงสุด
                if (item.Quantity < maxStock)
                {
                    item.Quantity++;
                    updateNeeded = true;
                }
                else
                {
                    MessageBox.Show($"ไม่สามารถเพิ่มได้อีก: สินค้านี้มีในคลังสูงสุด {maxStock} ชิ้น", "สินค้าเต็มจำนวน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            //  คลิกปุ่มลด (-)
            else if (dgvSummary.Columns[e.ColumnIndex].Name == "MinusCol")
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                    updateNeeded = true;
                }
                else // ถ้าเหลือ 1 ให้ถามยืนยันการลบ
                {
                    DialogResult result = MessageBox.Show($"ต้องการลบ {item.Name} ออกจากตะกร้าหรือไม่?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        _cartItems.RemoveAt(e.RowIndex);
                        updateNeeded = true;
                    }
                }
            }

            if (updateNeeded)
            {
                //  รีเฟรชหน้าจอ 
                DisplayCartSummary();
            }
        }

        /// <summary>
        /// ดึงจำนวนสต็อกทั้งหมดของสินค้าจากตาราง products
        /// </summary>
        private int GetMaxStockQuantity(int productId)
        {
           

            string query = "SELECT StockQuantity FROM products WHERE ProductId = @Id";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", productId);
                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    // แปลงเป็น int
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("ข้อผิดพลาดในการตรวจสอบสต็อก: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return 0; // คืนค่า 0 หากเกิดข้อผิดพลาดหรือสินค้าไม่พบ
            }
        }

        /// <summary>
        /// เมธอด Helper สำหรับคำนวณและอัปเดตยอดรวมสุทธิ
        /// </summary>
        private void UpdateTotalSummary()
        {
            // 1. คำนวณยอดรวมก่อน VAT (Subtotal)
            decimal subTotal = _cartItems.Sum(item => item.Total);

            // 2. คำนวณ VAT 7%
            const decimal VAT_RATE = 0.07m; // 7%
            decimal vatAmount = subTotal * VAT_RATE;

            // 3. คำนวณยอดรวมสุทธิทั้งหมด (Grand Total)
            decimal grandTotal = subTotal + vatAmount;

            // 4. แสดงผล (ถ้าคุณมี Label หลายตัวสำหรับแสดง Subtotal และ VAT จะดีกว่า)

            // แสดงเฉพาะยอดรวมสุทธิทั้งหมด (Total + VAT) ใน lblTotal
            lblTotal.Text = $"ยอดรวมสุทธิ: ฿{grandTotal:N2} (รวม VAT 7%)";
        }

       

        private void lblTotal_Click(object sender, EventArgs e)
        {

        }

        private void btnConfirmOrder_Click(object sender, EventArgs e)
        {
            //  เปิดฟอร์ม PaymentForm
            using (PaymentForm paymentForm = new PaymentForm())
            {
                if (paymentForm.ShowDialog() == DialogResult.OK)
                {
                    //  ดึงค่า Address และ Slip Path จาก PaymentForm
                    string address = paymentForm.ShippingAddress; 
                    string slipPath = paymentForm.PaymentSlipPath; 

                    //  บันทึกค่าลงใน Public Properties ของ CheckoutForm
                    this.ShippingAddress = address; 
                    this.PaymentSlipPath = slipPath; 

                    // 3. ส่งสัญญาณสำเร็จกลับไป mainpage เพื่อเริ่ม ProcessCheckout
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }
    }
}
