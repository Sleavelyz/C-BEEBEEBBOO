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
    public partial class mainpage : Form
    {
        private int currentUserId;
        private const string connectionString = "server=127.0.0.1;port=3306;database=bee;uid=root;password=;";
        private List<CartItem> shoppingCart = new List<CartItem>();
       
        public int CurrentUserId { get; set; } =0;
        public class CartItem
        {
            public int ProductId { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public decimal Total => Price * Quantity;
        }
        public mainpage(int userId)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            currentUserId = userId; // เก็บ ID ไว้ใช้ในเมธอดอื่น ๆ
            this.Text = "Main Page - User ID: " + currentUserId.ToString();
            LoadProductsForDisplay(null); 
            UpdateCartDisplay();
        }

        private void mainpage_Load(object sender, EventArgs e)
        {
            //  กำหนดขนาดสูงสุดของฟอร์ม
            // Screen.PrimaryScreen.WorkingArea คือ พื้นที่หน้าจอที่ไม่รวม Taskbar
            this.MaximumSize = Screen.PrimaryScreen.WorkingArea.Size;

            //  ตั้งค่า WindowState เป็น Maximized
            
            this.WindowState = FormWindowState.Maximized;

            //  ปรับขนาดฟอร์มให้เต็มพื้นที่ทำงานอีกครั้ง (บางครั้งจำเป็น)
            this.Size = Screen.PrimaryScreen.WorkingArea.Size;
        }
        // ======================================================
        //  โหลดและแสดงสินค้า (Display Logic)
        // ======================================================

        /// <summary>
        /// โหลดรายการสินค้าจากฐานข้อมูล (กรองตาม CategoryId ถ้ามี)
        /// </summary>
        /// <param name="categoryId">CategoryId ที่ต้องการกรอง (0 หรือ null สำหรับทั้งหมด)</param>
        private void LoadProductsForDisplay(int? categoryId = null)
        {
            List<Product> products = new List<Product>();

            //  สร้าง Query ฐาน
            string query = "SELECT ProductId, Name, Description, Price, StockQuantity, ImagePath, CategoryId FROM products WHERE StockQuantity > 0";

            //  เพิ่มเงื่อนไขการกรอง
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query += " AND CategoryId = @CatId";
            }

          

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                //  ผูก Parameter หากมีการกรอง
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    command.Parameters.AddWithValue("@CatId", categoryId.Value);
                }

                try
                {
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                ProductId = reader.GetInt32("ProductId"),
                                Name = reader.GetString("Name"),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString("Description"),
                                Price = reader.GetDecimal("Price"),
                                StockQuantity = reader.GetInt32("StockQuantity"),
                                ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? null : reader.GetString("ImagePath"),
                                CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId")) ? (int?)null : reader.GetInt32("CategoryId")
                            });
                        }
                    }
                    // เรียกใช้เมธอด DisplayProducts เพื่อสร้างการ์ดสินค้า
                    DisplayProducts(products);
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("ข้อผิดพลาดในการโหลดสินค้า:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// สร้าง Product Card และเพิ่มเข้าใน FlowLayoutPanel
        /// </summary>
        private void DisplayProducts(List<Product> products)
        {
            productsPanel.Controls.Clear();

            foreach (var product in products)
            {
                //  คำนวณจำนวนสินค้าที่อยู่ในตะกร้าแล้ว
                int cartCount = shoppingCart.Where(item => item.ProductId == product.ProductId).Sum(item => item.Quantity);

                //  คำนวณสต็อกที่เหลือให้สั่งเพิ่มได้
                int availableStock = product.StockQuantity - cartCount;

                //  สร้าง UCProductCard โดยส่งสต็อกที่เหลือไปให้
                UCProductCard card = new UCProductCard(product, availableStock); 

                card.OnAddToCart += AddToCart_Handler;
                card.Margin = new Padding(15, 15, 15, 15);
                productsPanel.Controls.Add(card);
            }
        }

        // ======================================================
        //  ตะกร้าสินค้าและการจัดการ
        // ======================================================

        private void AddToCart(Product product, int quantity)
        {
            var existingItem = shoppingCart.FirstOrDefault(item => item.ProductId == product.ProductId);

            //  คำนวณจำนวนสินค้าที่ต้องการจะมีในตะกร้าหลังจากการเพิ่ม
            int newTotalQuantity = quantity;
            if (existingItem != null)
            {
                newTotalQuantity = existingItem.Quantity + quantity;
            }

            //  ตรวจสอบสต็อกสะสม (Cumulative Stock Check)
            if (newTotalQuantity > product.StockQuantity)
            {
                // แจ้งเตือนและหยุดการทำงาน (นี่คือส่วนที่แก้ไขปัญหาไม่ให้เพิ่มสินค้าเกิน)
                MessageBox.Show(
                    $"ไม่สามารถเพิ่มสินค้าได้: จำนวนสูงสุดในคลังคือ {product.StockQuantity} ชิ้น " +
                    $"(มีอยู่ในตะกร้าแล้ว {existingItem?.Quantity ?? 0} ชิ้น)",
                    "สินค้าไม่พอในคลัง",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return; //  หยุดการทำงานทันทีถ้าสต็อกไม่พอ
            }

            //  ถ้าผ่านการตรวจสอบ ให้ดำเนินการเพิ่มสินค้า
            if (existingItem != null)
            {
                existingItem.Quantity = newTotalQuantity; // อัปเดตจำนวนใหม่
            }
            else
            {
                shoppingCart.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity
                });
            }

            //  แจ้งเตือนความสำเร็จ 
            MessageBox.Show($"{product.Name} ถูกเพิ่มลงในตะกร้า {quantity} ชิ้นแล้ว!", "Cart Update", MessageBoxButtons.OK, MessageBoxIcon.Information);

            UpdateCartDisplay();
        }

        private void UpdateCartDisplay()
        {
           

            // ใช้เพื่อคำนวณยอดรวมเท่านั้น
            decimal cartTotal = shoppingCart.Sum(item => item.Total);

            
        }

        private void AddToCart_Handler(Product product, int quantity)
        {
            // เรียก AddToCart (ซึ่งจะจัดการ Logic ตรวจสอบและแสดง MessageBox)
            AddToCart(product, quantity);

            // สำคัญ: เรียกโหลดใหม่เพื่ออัปเดตสถานะปุ่ม 'เพิ่มลงตะกร้า' ของทุก Card
            LoadProductsForDisplay(null); // Load all products (หรือตาม filter ปัจจุบัน)
        }
        private void btnCheckout_Click(object sender, EventArgs e)
        {
            if (shoppingCart.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกสินค้าก่อนทำการสั่งซื้อ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (currentUserId == 0)
            {
                MessageBox.Show("ข้อผิดพลาด: ไม่พบ ID ผู้ใช้ กรุณาเข้าสู่ระบบก่อนสั่งซื้อ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // เปิดหน้าสรุป Checkout หรือดำเนินการสั่งซื้อทันที
            ShowCheckoutSummary();
        }
        private void ShowCheckoutSummary()
        {
            using (CheckoutForm checkoutForm = new CheckoutForm(shoppingCart, CurrentUserId))
            {
                if (checkoutForm.ShowDialog() == DialogResult.OK)
                {
                    //  ดึงข้อมูลที่อยู่และสลิปจาก CheckoutForm (ซึ่งตอนนี้มี Public Properties แล้ว)
                    string shippingAddress = checkoutForm.ShippingAddress;
                    string slipPath = checkoutForm.PaymentSlipPath;

                    //  เรียก ProcessCheckout (ซึ่งเปลี่ยนไปคืนค่า OrderId)
                    int orderId = ProcessCheckout(shippingAddress, slipPath, currentUserId);

                    // เปิดหน้า ReceiptForm
                    ReceiptForm receiptForm = new ReceiptForm(orderId);
                    receiptForm.ShowDialog(); // แสดงใบเสร็จ

                    //  หลังจากปิดใบเสร็จแล้วค่อยล้างตะกร้าและรีโหลดสินค้า
                    shoppingCart.Clear();
                    UpdateCartDisplay();
                    LoadProductsForDisplay(null);
                }
                else
                {
                    // หากผู้ใช้ปิดฟอร์มโดยไม่สั่งซื้อ (DialogResult.Cancel)
                    LoadProductsForDisplay(null);
                }
            }
        }
        /// <summary>
        /// ประมวลผลการสั่งซื้อโดยบันทึก orders, order_details, และลด stock 
        /// (เมธอดนี้จัดการเฉพาะ Transaction และคืนค่า OrderId)
        /// </summary>
        /// <returns>OrderId ที่สร้างขึ้นใหม่</returns>
        private int ProcessCheckout(string shippingAddress, string slipPath, int userId)
        {
            // 1. คำนวณยอดรวมสุทธิ (Subtotal)
            decimal subTotal = shoppingCart.Sum(item => item.Total);

            // 2. คำนวณ VAT 7%
            const decimal vatRate = 0.07m; // 7%
            decimal vatAmount = subTotal * vatRate; // คำนวณ VAT

            // 3. ยอดรวมทั้งหมดที่ต้องชำระ (รวม VAT)
            decimal totalAmountWithVAT = subTotal + vatAmount;

            int orderId = 0;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // =========================================================
                    //  INSERT INTO orders (บันทึกยอดรวมทั้งหมดรวม VAT)
                    // =========================================================
                    string insertOrderQuery = @"
                INSERT INTO orders (UserId, TotalAmount, Status, ShippingAddress, SlipImagePath, PaymentStatus)  
                VALUES (@UserId, @Total, 'Pending', @Address, @SlipPath, 'Pending Verification');
                SELECT LAST_INSERT_ID();";

                    MySqlCommand orderCommand = new MySqlCommand(insertOrderQuery, connection, transaction);
                    orderCommand.Parameters.AddWithValue("@UserId", userId);
                    orderCommand.Parameters.AddWithValue("@Total", totalAmountWithVAT); // *** ใช้ยอดรวมรวม VAT บันทึกลง TotalAmount ***
                    orderCommand.Parameters.AddWithValue("@Address", shippingAddress);
                    orderCommand.Parameters.AddWithValue("@SlipPath", slipPath);

                    // ดึงค่า OrderId
                    orderId = Convert.ToInt32(orderCommand.ExecuteScalar());


                    // =========================================================
                    //  INSERT INTO order_details และ UPDATE products
                    // =========================================================

                    foreach (var item in shoppingCart)
                    {
                        //  บันทึกรายละเอียดสินค้า
                        string insertDetailQuery = @"
                    INSERT INTO order_details (OrderId, ProductId, Quantity, Price)  
                    VALUES (@OrderId, @ProductId, @Quantity, @Price)";

                        MySqlCommand detailCommand = new MySqlCommand(insertDetailQuery, connection, transaction);
                        detailCommand.Parameters.AddWithValue("@OrderId", orderId);
                        detailCommand.Parameters.AddWithValue("@ProductId", item.ProductId);
                        detailCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                        detailCommand.Parameters.AddWithValue("@Price", item.Price);
                        detailCommand.ExecuteNonQuery();

                        //  ลด StockQuantity
                        string updateStockQuery = @"
                    UPDATE products  
                    SET StockQuantity = StockQuantity - @Quantity  
                    WHERE ProductId = @ProductId AND StockQuantity >= @Quantity";

                        MySqlCommand stockCommand = new MySqlCommand(updateStockQuery, connection, transaction);
                        stockCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                        stockCommand.Parameters.AddWithValue("@ProductId", item.ProductId);

                        if (stockCommand.ExecuteNonQuery() == 0)
                        {
                            // ถ้า Stock ไม่พอ ให้ยกเลิก Transaction
                            throw new Exception($"ไม่สามารถลดสต็อกสินค้า {item.Name} ได้: สินค้าคงเหลือไม่พอ");
                        }
                    }

                    // COMMIT (ยืนยันการเปลี่ยนแปลงทั้งหมด)
                    transaction.Commit();

                    return orderId; // คืนค่า OrderId เมื่อ Transaction สำเร็จ
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    
                    throw new Exception("เกิดข้อผิดพลาดระหว่างการสั่งซื้อ: " + ex.Message);
                }
            }
        }


        private void productsPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnFilterAll_Click(object sender, EventArgs e)
        {
            CategoryFilter_Click(sender, e);
        }

        private void btnFilterHoney_Click(object sender, EventArgs e)
        {
            CategoryFilter_Click(sender, e);
        }

        private void btnFilterProduct_Click(object sender, EventArgs e)
        {
            CategoryFilter_Click(sender, e);
        }

        private void btnFilterSupplement_Click(object sender, EventArgs e)
        {
            CategoryFilter_Click(sender, e);
        }

        private void btnFilterGift_Click(object sender, EventArgs e)
        {
            CategoryFilter_Click(sender, e);
        }

        private void CategoryFilter_Click(object sender, EventArgs e)
        {
           
            Guna.UI2.WinForms.Guna2Button btn = sender as Guna.UI2.WinForms.Guna2Button; 

            if (btn != null && btn.Tag != null && int.TryParse(btn.Tag.ToString(), out int categoryId))
            {
                // ... (Logic การกรองยังคงเดิม) ...
                if (categoryId == 0)
                {
                    LoadProductsForDisplay(null);
                }
                else
                {
                    LoadProductsForDisplay(categoryId);
                }
            }
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            // ยืนยันการออกจากระบบ
            DialogResult result = MessageBox.Show("คุณแน่ใจหรือไม่ว่าต้องการออกจากระบบ?", "ยืนยันการออกจากระบบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                //  สร้างอินสแตนซ์ของหน้า Login ใหม่
                login loginForm = new login();

                //  ซ่อน/ปิดฟอร์ม Mainpage ปัจจุบัน
                this.Hide();

                //  แสดงฟอร์ม Login
                loginForm.Show();

                
                this.Close();
            }
        }

        private void btnShowHistory_Click(object sender, EventArgs e)
        {
            if (currentUserId > 0)
            {
                // สร้างฟอร์มประวัติการซื้อ และส่ง User ID ไป
                OrderHistoryForm historyForm = new OrderHistoryForm(currentUserId);
                historyForm.ShowDialog(); // ใช้ ShowDialog เพื่อให้ผู้ใช้จัดการฟอร์มนี้ก่อน
            }
            else
            {
                MessageBox.Show("ไม่สามารถโหลดประวัติการซื้อได้ เนื่องจากไม่พบ User ID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
