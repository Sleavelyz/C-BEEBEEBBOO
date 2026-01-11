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
using System.IO; // สำหรับการจัดการไฟล์รูปภาพ
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Bee
{
    public partial class ProductForm : Form
    {
        private const string connectionString =
            "server=127.0.0.1;port=3306;database=bee;uid=root;password=;";

        private int _productId;
        private string _imagePath; // เก็บ Path รูปภาพปัจจุบัน
        public ProductForm(int productId)
        {
            InitializeComponent();
            _productId = productId;
            this.StartPosition = FormStartPosition.CenterParent;

            LoadCategories(); // <--- เรียกใช้ Load Categories ที่นี่

            if (_productId != 0)
            {
                this.Text = "แก้ไขสินค้า";
                LoadProductData(_productId); // โหลดข้อมูลเดิมถ้าเป็นการแก้ไข
            }
            else
            {
                this.Text = "เพิ่มสินค้าใหม่";
            }
        }

        private void ProductForm_Load(object sender, EventArgs e)
        {

        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtPrice_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtStock_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtDescription_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnChooseImage_Click(object sender, EventArgs e)
        {
            // ใช้ OpenFileDialog เพื่อให้ผู้ใช้เลือกไฟล์รูปภาพ
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                // กรองให้เลือกได้เฉพาะไฟล์รูปภาพหลักๆ
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif|All Files (*.*)|*.*";
                ofd.Title = "เลือกรูปภาพสำหรับสินค้า";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        //  บันทึก Path ของรูปภาพใหม่
                        _imagePath = ofd.FileName;

                        //  แสดงรูปภาพใน PictureBox
                        // (ต้องตรวจสอบให้แน่ใจว่าคุณมี PictureBox ชื่อ 'pbImage' ใน Designer)
                        pbImage.Image = Image.FromFile(_imagePath);
                        pbImage.SizeMode = PictureBoxSizeMode.Zoom; // ตั้งค่าการแสดงผลรูปภาพ

                        // NOTE:
                        // การบันทึกเพียงแค่ Path (Local Path) นี้อาจไม่ทำงานหากนำไปเปิด
                        // บนเครื่องอื่น หากต้องการระบบเต็มรูปแบบ
                        // คุณควรคัดลอกไฟล์รูปภาพนี้ไปยังโฟลเดอร์ของโปรเจกต์ 
                        // แล้วบันทึก Path สัมพัทธ์ (Relative Path) แทน

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ข้อผิดพลาดในการโหลดรูปภาพ:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            //  ดึง CategoryId จาก ComboBox
            // ตรวจสอบว่ามีค่าถูกเลือกหรือไม่
            if (cmbCategory.SelectedValue == null || cmbCategory.SelectedIndex == -1)
            {
                MessageBox.Show("กรุณาเลือกประเภทสินค้า", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int? selectedCategoryId = (int?)cmbCategory.SelectedValue;

            Product product = new Product
            {
                ProductId = _productId,
                Name = txtName.Text.Trim(),
                Price = decimal.Parse(txtPrice.Text),
                StockQuantity = int.Parse(txtStock.Text),
                Description = txtDescription.Text,
                ImagePath = _imagePath,
                CategoryId = selectedCategoryId // <--- ใส่ Category ID ที่เลือก
            };

            if (SaveProductToDatabase(product))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        /// <summary>
        /// โหลดข้อมูลสินค้าเดิมจาก DB เข้าสู่ Controls
        /// </summary>
        private void LoadProductData(int id)
        {
            string query = "SELECT Name, Description, Price, StockQuantity, ImagePath, CategoryId FROM products WHERE ProductId = @Id";

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
                           
                            txtName.Text = reader.GetString("Name");
                            txtPrice.Text = reader.GetDecimal("Price").ToString();
                            txtStock.Text = reader.GetInt32("StockQuantity").ToString();
                            txtDescription.Text = reader.IsDBNull(reader.GetOrdinal("Description")) ? string.Empty : reader.GetString("Description");

                            // จัดการรูปภาพ
                            _imagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? null : reader.GetString("ImagePath");
                            if (!string.IsNullOrEmpty(_imagePath) && File.Exists(_imagePath))
                            {
                                // ตรวจสอบว่าคุณมี PictureBox ชื่อ pbImage
                                pbImage.Image = Image.FromFile(_imagePath);
                            }
                            int? categoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId")) ? (int?)null : reader.GetInt32("CategoryId");
                            if (categoryId.HasValue)
                            {
                                cmbCategory.SelectedValue = categoryId.Value;
                            }
                            else
                            {
                                cmbCategory.SelectedIndex = -1;
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("ข้อผิดพลาดในการโหลดข้อมูลสินค้า:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    // อาจมีข้อผิดพลาดเกี่ยวกับการโหลดไฟล์รูปภาพ
                    MessageBox.Show("ข้อผิดพลาดทั่วไปในการแสดงข้อมูล:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// ตรวจสอบความถูกต้องของข้อมูลที่ผู้ใช้กรอก
        /// </summary>
        private bool ValidateInput()
        {
            //  ตรวจสอบข้อมูลว่าง
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPrice.Text) || string.IsNullOrWhiteSpace(txtStock.Text))
            {
                MessageBox.Show("กรุณากรอกชื่อ ราคา และจำนวนคงเหลือให้ครบถ้วน", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            //  ตรวจสอบรูปแบบราคาสินค้า
            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("ราคาสินค้าต้องเป็นตัวเลขที่ถูกต้องและมากกว่าศูนย์", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            //  ตรวจสอบรูปแบบจำนวนคงเหลือ
            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("จำนวนคงเหลือต้องเป็นจำนวนเต็มที่ไม่ติดลบ", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
        private void pbImage_Click(object sender, EventArgs e)
        {

        }

        private bool SaveProductToDatabase(Product product)
        {
            string query = (product.ProductId == 0) ?
                // INSERT Query ยังคงถูกต้อง
                @"INSERT INTO products (Name, Description, Price, StockQuantity, ImagePath, CategoryId, LowStockThreshold)  
                 VALUES (@Name, @Desc, @Price, @Stock, @Image, @CatId, 10)" :
                // 🔑 UPDATE Query ต้องเพิ่ม CategoryId
                @"UPDATE products SET Name = @Name, Description = @Desc, Price = @Price,  
                 StockQuantity = @Stock, ImagePath = @Image, CategoryId = @CatId  
                 WHERE ProductId = @Id";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                // กำหนด Parameters (ใช้โค้ดส่วนนี้จากที่เราทำใน UCProductManagement)
                command.Parameters.AddWithValue("@Name", product.Name);
                command.Parameters.AddWithValue("@CatId", product.CategoryId ?? (object)DBNull.Value); // <--- เพิ่มบรรทัดนี้
                command.Parameters.AddWithValue("@Desc", product.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Price", product.Price);
                command.Parameters.AddWithValue("@Stock", product.StockQuantity);
                command.Parameters.AddWithValue("@Image", product.ImagePath ?? (object)DBNull.Value);
                if (product.ProductId != 0) command.Parameters.AddWithValue("@Id", product.ProductId);

                try
                {
                    connection.Open();
                    return command.ExecuteNonQuery() > 0;
                }
                catch (MySqlException ex)
                {
                    if (ex.Number == 1062) MessageBox.Show("ชื่อสินค้าซ้ำ!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    else MessageBox.Show("ข้อผิดพลาดของฐานข้อมูล:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }
        /// <summary>
        /// อนุญาตให้ป้อนตัวเลข, backspace, และจุดทศนิยม (สำหรับราคา) เท่านั้น
        /// </summary>
        private void TextBox_Numeric_KeyPress(object sender, KeyPressEventArgs e)
        {
            // ตรวจสอบว่าเป็น Control ที่ต้องการทศนิยมหรือไม่ (เช่น ราคา)
            bool allowDecimal = (sender == txtPrice);

            // อนุญาตให้พิมพ์ Backspace (ASCII 8)
            if (e.KeyChar == (char)Keys.Back)
            {
                e.Handled = false;
                return;
            }

            // อนุญาตให้พิมพ์ตัวเลข 0-9
            if (char.IsDigit(e.KeyChar))
            {
                e.Handled = false;
                return;
            }

            // อนุญาตให้พิมพ์จุดทศนิยม (เฉพาะฟิลด์ราคา และอนุญาตแค่ 1 จุด)
            if (allowDecimal && e.KeyChar == '.')
            {
                // ตรวจสอบว่ามีจุดอยู่แล้วหรือไม่
                if (((TextBox)sender).Text.Contains('.'))
                {
                    e.Handled = true; // มีจุดแล้ว, ไม่อนุญาตเพิ่ม
                }
                else
                {
                    e.Handled = false;
                }
                return;
            }

            // บล็อกอักขระอื่นๆ
            e.Handled = true;
        }

        private void txtStock_KeyPress(object sender, KeyPressEventArgs e)
        {
           
        }

        private void cmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// โหลดรายการประเภทสินค้าจากฐานข้อมูลเข้าสู่ ComboBox
        /// </summary>
        private void LoadCategories()
        {
            

            string query = "SELECT CategoryId, Name FROM categories ORDER BY Name";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                try
                {
                    connection.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // ผูก DataTable เข้ากับ ComboBox
                    cmbCategory.DataSource = dt;
                    cmbCategory.DisplayMember = "Name"; // สิ่งที่ผู้ใช้เห็น (เช่น 'น้ำผึ้ง')
                    cmbCategory.ValueMember = "CategoryId"; // ค่าจริงที่โค้ดใช้ (เช่น 1, 2, 3...)
                    cmbCategory.SelectedIndex = -1; // ไม่เลือกอะไรไว้ก่อน
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("ข้อผิดพลาดในการโหลดประเภทสินค้า:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
