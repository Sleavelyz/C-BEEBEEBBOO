using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bee
{
    public partial class ReceiptForm : Form
    {
        private PrintDocument printDoc;
        private const string connectionString = "server=127.0.0.1;port=3306;database=bee;uid=root;password=;";

        private int _orderId;
        private decimal _totalAmount;
        private string _shippingAddress;
        private string _slipPath;
        private string _username; // 

        // NOTE: _items ใช้ class CartItem จาก mainpage.cs
        private List<mainpage.CartItem> _items = new List<mainpage.CartItem>();

        //
        public ReceiptForm(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;

            LoadReceiptDetails(); // ดึงข้อมูลจาก DB

            this.StartPosition = FormStartPosition.CenterScreen;

            //  สร้างอินสแตนซ์ PrintDocument
            printDoc = new PrintDocument();
            //  ผูก Event PrintPage เข้ากับเมธอดที่จะวาดใบเสร็จ
            printDoc.PrintPage += new PrintPageEventHandler(this.printDoc_PrintPage);
        }
        

        private void ReceiptForm_Load(object sender, EventArgs e)
        {

        }

        private void txtReceiptDetails_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnDownloadReceipt_Click(object sender, EventArgs e)
        {
            // Logic สำหรับ Print to PDF
            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDoc;

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDoc.Print();
            }
        }

        private void SaveReceiptToFile()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                // 🔑 ตั้งค่า Filter ให้เหลือแค่ PDF (*.pdf)
                sfd.Filter = "PDF Files (*.pdf)|*.pdf"; // <--- แก้ไขบรรทัดนี้
                sfd.Title = "บันทึกใบเสร็จเป็น PDF";
                sfd.FileName = $"Receipt_{_orderId}.pdf"; // เปลี่ยนนามสกุลไฟล์เป็น .pdf

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // 🛑 ณ จุดนี้ คุณต้องใช้ไลบรารีภายนอกในการสร้างไฟล์ PDF 🛑

                        // ตัวอย่าง: ถ้าใช้ iTextSharp หรือไลบรารีอื่น
                        // GeneratePdf(sfd.FileName, txtReceiptDetails.Text); 

                        // หรือถ้าต้องการบันทึกเป็น Text แต่แสดงผลเป็น PDF ใน dialog (ไม่แนะนำ)
                        // File.WriteAllText(sfd.FileName, txtReceiptDetails.Text); 

                        MessageBox.Show("บันทึกใบเสร็จสำเร็จ!", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ข้อผิดพลาดในการบันทึกไฟล์: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LoadReceiptDetails()
        {
            string orderQuery = @"
                SELECT o.OrderDate, o.TotalAmount, o.ShippingAddress, o.SlipImagePath,
                       od.ProductId, od.Quantity, od.Price, p.Name, u.username
                FROM orders o
                JOIN order_details od ON o.OrderId = od.OrderId
                JOIN products p ON od.ProductId = p.ProductId
                JOIN users u ON o.UserId = u.id
                WHERE o.OrderId = @OrderId";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(orderQuery, connection))
            {
                command.Parameters.AddWithValue("@OrderId", _orderId);

                try
                {
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows) return;

                        //  ดึงข้อมูลหลัก (ทำเพียงครั้งเดียว)
                        reader.Read();
                        _totalAmount = reader.GetDecimal("TotalAmount");
                        _shippingAddress = reader.IsDBNull(reader.GetOrdinal("ShippingAddress")) ? "ไม่ระบุ" : reader.GetString("ShippingAddress");
                        _slipPath = reader.IsDBNull(reader.GetOrdinal("SlipImagePath")) ? null : reader.GetString("SlipImagePath");
                        DateTime orderDate = reader.GetDateTime("OrderDate");
                        _username = reader.GetString("username");

                        //  วนลูปดึงรายละเอียดสินค้าทั้งหมด
                        _items.Clear();
                        do
                        {
                            _items.Add(new mainpage.CartItem
                            {
                                Name = reader.GetString("Name"),
                                Quantity = reader.GetInt32("Quantity"),
                                Price = reader.GetDecimal("Price")
                            });
                        } while (reader.Read());

                        //  แสดงผลใน RichTextBox (UI)
                        DisplayReceipt(orderDate);
                    }
                }
                catch (MySqlException ex)
                {
                    txtReceiptDetails.Text = $"เกิดข้อผิดพลาดในการโหลดข้อมูล: {ex.Message}";
                }
            }
        }

        private void DisplayReceipt(DateTime orderDate)
        {
            decimal subTotal = _items.Sum(item => item.Total); // ยอดรวมสินค้าก่อน VAT
            const decimal VAT_RATE = 0.07m;
            decimal vatAmount = subTotal * VAT_RATE;
            decimal grandTotal = subTotal + vatAmount;
            _totalAmount = grandTotal;

            // Logic สำหรับแสดงผลใน RichTextBox (ReadOnly)
            StringBuilder receiptBuilder = new StringBuilder();
            receiptBuilder.AppendLine("==========================================");
            receiptBuilder.AppendLine("           BEEBEEBBOO STORE");
            receiptBuilder.AppendLine($"   ใบเสร็จรับเงิน (ORDER ID: {_orderId})");
            receiptBuilder.AppendLine("==========================================");
            receiptBuilder.AppendLine($"ผู้สั่งซื้อ: {_username}");
            receiptBuilder.AppendLine($"วันที่สั่งซื้อ: {orderDate.ToString("dd/MM/yyyy HH:mm:ss")}");
            receiptBuilder.AppendLine($"ที่อยู่จัดส่ง: {_shippingAddress}");
            receiptBuilder.AppendLine("------------------------------------------");
            receiptBuilder.AppendLine("รายการสินค้า:");

            foreach (var item in _items)
            {
                receiptBuilder.AppendLine($"  - {item.Name}");
                receiptBuilder.AppendLine($"    จำนวน: {item.Quantity} ชิ้น @ ฿{item.Price:N2} = ฿{item.Total:N2}");
            }

            receiptBuilder.AppendLine("------------------------------------------");
            receiptBuilder.AppendLine($"ยอดรวมสินค้า: ฿{subTotal:N2}");
            receiptBuilder.AppendLine($"ภาษีมูลค่าเพิ่ม (VAT 7%): ฿{vatAmount:N2}");

            receiptBuilder.AppendLine("------------------------------------------");
            receiptBuilder.AppendLine($"สถานะชำระเงิน: Pending Verification");
            receiptBuilder.AppendLine($"ยอดรวมสุทธิ (รวม VAT): ฿{grandTotal:N2}");
            receiptBuilder.AppendLine("==========================================");

            txtReceiptDetails.Text = receiptBuilder.ToString();
            txtReceiptDetails.ReadOnly = true; // ป้องกันการแก้ไข
        }

        private void printDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            Font titleFont = new Font("Arial", 14, FontStyle.Bold);
            Font boldFont = new Font("Arial", 10, FontStyle.Bold);
            Font normalFont = new Font("Arial", 10);
            SolidBrush blackBrush = new SolidBrush(Color.Black);

            float x = e.MarginBounds.Left;
            float y = e.MarginBounds.Top;
            float lineHeight = normalFont.GetHeight() + 5;

            decimal subTotal = _items.Sum(item => item.Total);
            const decimal VAT_RATE = 0.07m;
            decimal vatAmount = subTotal * VAT_RATE;
            decimal grandTotal = subTotal + vatAmount;


            string storeName = "BEEBEEBBOO STORE";
            e.Graphics.DrawString(storeName, titleFont, blackBrush, x, y); // วาดชื่อร้าน
            y += lineHeight;

            //  หัวข้อใบเสร็จและข้อมูลหลัก
            e.Graphics.DrawString($"ใบเสร็จรับเงิน (ORDER ID: {_orderId})", titleFont, blackBrush, x, y);
            y += lineHeight * 2;

            e.Graphics.DrawString($"ผู้สั่งซื้อ: {_username}", normalFont, blackBrush, x, y);
            y += lineHeight;
            // NOTE: ใช้ DateTime.Now ใน PrintPage เพราะไม่ได้รับ orderDate เป็นพารามิเตอร์
            e.Graphics.DrawString($"วันที่พิมพ์: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", normalFont, blackBrush, x, y);
            y += lineHeight;

            e.Graphics.DrawString($"ที่อยู่จัดส่ง: {_shippingAddress}", normalFont, blackBrush, x, y);
            y += lineHeight * 2;

            //  รายการสินค้า (Itemized List)
            string itemsHeader = "=================== รายการสินค้า ===================";
            e.Graphics.DrawString(itemsHeader, boldFont, blackBrush, x, y);
            y += lineHeight;

            // หัวคอลัมน์
            e.Graphics.DrawString("สินค้า", boldFont, blackBrush, x, y);
            e.Graphics.DrawString("จำนวน", boldFont, blackBrush, x + 250, y);
            e.Graphics.DrawString("รวม", boldFont, blackBrush, x + 400, y);
            y += lineHeight;

            
            y += 5;

            foreach (var item in _items)
            {
                // รายละเอียดสินค้า
                e.Graphics.DrawString(item.Name, normalFont, blackBrush, x, y);

                // จำนวน
                e.Graphics.DrawString(item.Quantity.ToString(), normalFont, blackBrush, x + 260, y);

                // ราคารวม
                string totalString = $"฿{item.Total:N2}";
                e.Graphics.DrawString(totalString, normalFont, blackBrush, x + 400, y);

                y += lineHeight;
            }

            y += lineHeight * 2;
            e.Graphics.DrawString("----------------------------------------------------------", normalFont, blackBrush, x, y);
            y += lineHeight;

            // 🔑 แสดงยอดรวมย่อย
            e.Graphics.DrawString("ยอดรวม (ก่อน VAT):", normalFont, blackBrush, x, y);
            e.Graphics.DrawString($"฿{subTotal:N2}", normalFont, blackBrush, x + 400, y);
            y += lineHeight;

            // 🔑 แสดง VAT
            e.Graphics.DrawString("VAT 7%:", normalFont, blackBrush, x, y);
            e.Graphics.DrawString($"฿{vatAmount:N2}", normalFont, blackBrush, x + 400, y);
            y += lineHeight;

            // 🔑 ยอดรวมสุทธิ (ใช้ grandTotal)
            e.Graphics.DrawString("==========================================", normalFont, blackBrush, x, y);
            y += lineHeight;
            e.Graphics.DrawString($"ยอดรวมสุทธิ: ฿{grandTotal:N2}", titleFont, blackBrush, x, y);
        }
    }
}
    