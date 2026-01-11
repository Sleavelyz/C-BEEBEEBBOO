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
    public partial class UCProductManagement : UserControl
    {
        private const string connectionString =
            "server=127.0.0.1;port=3306;database=bee;uid=root;password=;";
        public UCProductManagement()
        {
            InitializeComponent();
            // เมื่อ User Control ถูกสร้าง ให้โหลดข้อมูลสินค้าทันที
            
            LoadProductsToDataGridView();
        }

        private void UCProductManagement_Load(object sender, EventArgs e)
        {
            LoadCategories();
        }

        private void LoadProductsToDataGridView(string searchTerm = null, int categoryId = 0)
        {
            List<Product> products = new List<Product>();

            StringBuilder queryBuilder = new StringBuilder(@"
        SELECT p.ProductId, p.Name, p.Description, p.Price, p.StockQuantity, p.ImagePath, p.CategoryId, c.Name as CategoryName  
        FROM products p 
        LEFT JOIN categories c ON p.CategoryId = c.CategoryId
        WHERE 1 = 1");

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(queryBuilder.ToString(), connection))
            {
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    queryBuilder.Append(" AND p.Name LIKE @SearchTerm");
                    command.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                }

                if (categoryId > 0)
                {
                    queryBuilder.Append(" AND p.CategoryId = @CatId");
                    command.Parameters.AddWithValue("@CatId", categoryId);
                }

                
                command.CommandText = queryBuilder.ToString();

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
                                Description = reader.GetString("Description"),
                                Price = reader.GetDecimal("Price"),
                                StockQuantity = reader.GetInt32("StockQuantity"),
                                ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? null : reader.GetString("ImagePath"),
                                CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId")) ? (int?)null : reader.GetInt32("CategoryId"),
                                CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? "ไม่ระบุ" : reader.GetString("CategoryName"),
                            });
                        }
                    }

                    dataGridViewProducts.DataSource = products;
                    dataGridViewProducts.Columns.Clear(); // ล้างคอลัมน์ที่ถูกสร้างอัตโนมัติทั้งหมด (เพื่อควบคุมการแสดงผล)
                    dataGridViewProducts.AutoGenerateColumns = false; // ปิดการสร้างคอลัมน์อัตโนมัติ

                    // =========================================================
                    //  เพิ่มคอลัมน์รูปภาพและข้อมูล (ตามลำดับที่ต้องการ)
                    // =========================================================

                    //  คอลัมน์รูปภาพ 
                    DataGridViewImageColumn imageCol = new DataGridViewImageColumn();
                    imageCol.Name = "ProductImage";
                    imageCol.HeaderText = "รูปภาพ";
                    imageCol.DataPropertyName = "ImageDisplay"; 
                    imageCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
                    imageCol.Width = 100;
                    dataGridViewProducts.Columns.Add(imageCol);
                    dataGridViewProducts.RowTemplate.Height = 80; // ตั้งความสูงแถว

                    //  คอลัมน์ข้อมูลอื่นๆ
                    dataGridViewProducts.Columns.Add("Name", "ชื่อสินค้า");
                    dataGridViewProducts.Columns["Name"].DataPropertyName = "Name";

                    dataGridViewProducts.Columns.Add("CategoryName", "ประเภทสินค้า"); 
                    dataGridViewProducts.Columns["CategoryName"].DataPropertyName = "CategoryName";

                    dataGridViewProducts.Columns.Add("Price", "ราคา");
                    dataGridViewProducts.Columns["Price"].DataPropertyName = "Price";

                    dataGridViewProducts.Columns.Add("StockQuantity", "คงเหลือ");
                    dataGridViewProducts.Columns["StockQuantity"].DataPropertyName = "StockQuantity";

                    dataGridViewProducts.Columns.Add("Status", "สถานะ");
                    dataGridViewProducts.Columns["Status"].DataPropertyName = "Status";

                    // =========================================================
                    //  เพิ่มคอลัมน์ปุ่ม (แก้ไข/ลบ) ที่ด้านหลังสุด
                    // =========================================================

                    //  เพิ่มปุ่มแก้ไข (Edit Button)
                    DataGridViewButtonColumn editButtonCol = new DataGridViewButtonColumn();
                    editButtonCol.Name = "EditButton";
                    editButtonCol.HeaderText = "แก้ไข";
                    editButtonCol.Text = "✏️ แก้ไข";
                    editButtonCol.UseColumnTextForButtonValue = true;
                    editButtonCol.Width = 80;
                    dataGridViewProducts.Columns.Add(editButtonCol);

                    // D. เพิ่มปุ่มลบ (Delete Button)
                    DataGridViewButtonColumn deleteButtonCol = new DataGridViewButtonColumn();
                    deleteButtonCol.Name = "DeleteButton";
                    deleteButtonCol.HeaderText = "ลบ";
                    deleteButtonCol.Text = "🗑️ ลบ";
                    deleteButtonCol.UseColumnTextForButtonValue = true;
                    deleteButtonCol.Width = 80;
                    dataGridViewProducts.Columns.Add(deleteButtonCol);


                    // =========================================================
                    //  ซ่อนคอลัมน์ Model ID และ Path 
                    // =========================================================

                    // ต้องเพิ่มคอลัมน์ ProductId กลับเข้าไปใน Columns Collection (แบบซ่อน)
                    // เพื่อให้เราดึงค่า ProductId ไปใช้ใน CellContentClick ได้
                    DataGridViewTextBoxColumn idCol = new DataGridViewTextBoxColumn();
                    idCol.Name = "ProductId";
                    idCol.DataPropertyName = "ProductId";
                    idCol.Visible = false;
                    dataGridViewProducts.Columns.Add(idCol);

                    // ซ่อนคอลัมน์อื่น ๆ ที่ไม่ต้องการแสดงผล (เช่น ImagePath ที่ยังอยู่)
                    if (dataGridViewProducts.Columns.Contains("ImagePath")) dataGridViewProducts.Columns["ImagePath"].Visible = false;
                    if (dataGridViewProducts.Columns.Contains("ImageDisplay")) dataGridViewProducts.Columns["ImageDisplay"].Visible = false;
                    if (dataGridViewProducts.Columns.Contains("CategoryId")) dataGridViewProducts.Columns["CategoryId"].Visible = false;

                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("ข้อผิดพลาดในการดึงข้อมูลสินค้า:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ======================================================
        // DELETE (การลบสินค้า)
        // ======================================================

        private void DeleteProduct(int productId)
        {
            // ... (โค้ดสำหรับ DeleteProduct เหมือนที่เราเขียนไปก่อนหน้านี้) ...
            var confirmResult = MessageBox.Show("คุณแน่ใจหรือไม่ว่าต้องการลบสินค้านี้?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                string query = "DELETE FROM products WHERE ProductId = @Id";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", productId);

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("ลบสินค้าสำเร็จ", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadProductsToDataGridView(); // รีโหลดข้อมูลหลังลบ
                        }
                        else
                        {
                            MessageBox.Show("ไม่พบสินค้านี้ในระบบ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        // UI Events: การจัดการปุ่มใน User Control
        // ======================================================

        

        private void dataGridViewProducts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int productId = (int)dataGridViewProducts.Rows[e.RowIndex].Cells["ProductId"].Value;

                // สมมติ: คอลัมน์ที่ e.ColumnIndex มีชื่อว่า "EditButton"
                if (dataGridViewProducts.Columns[e.ColumnIndex].Name == "EditButton")
                {
                    ShowProductForm(productId); // แก้ไขสินค้า
                }

                // สมมติ: คอลัมน์ที่ e.ColumnIndex มีชื่อว่า "DeleteButton"
                if (dataGridViewProducts.Columns[e.ColumnIndex].Name == "DeleteButton")
                {
                    DeleteProduct(productId); // ลบสินค้า
                }
            }
        }

        /// <summary>
        /// เปิดฟอร์ม Popup เพิ่ม/แก้ไขสินค้า
        /// </summary>
        /// <param name="productId">ID สินค้า (0 สำหรับเพิ่มใหม่)</param>
        private void ShowProductForm(int productId)
        {
            // ต้องสร้างคลาส ProductForm.cs ก่อน
            using (ProductForm form = new ProductForm(productId))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadProductsToDataGridView(); // รีโหลดข้อมูลเมื่อบันทึกสำเร็จ
                }
            }
        }

        private void btnAddProduct_Click_1(object sender, EventArgs e)
        {
            // 0 หมายถึงเพิ่มสินค้าใหม่
            ShowProductForm(0);
        }

        private void cmbCategoryFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        

        private void ApplyFilter()
        {
            //  ดึงค่าการค้นหาและค่า Filter ที่เลือก
            string searchTerm = txtSearch.Text.Trim();

            
            // ใช้ SelectedValue?.ToString() เพื่อหลีกเลี่ยงการ Cast เมื่อค่าเป็น null

            int selectedCategoryId = 0;
            if (cmbCategoryFilter.SelectedValue != null)
            {
                // ใช้ TryParse เพื่อแปลงค่า ValueMember (ซึ่งควรเป็น int)
                int.TryParse(cmbCategoryFilter.SelectedValue.ToString(), out selectedCategoryId);
            }
            // ถ้า TryParse ล้มเหลว หรือ SelectedValue เป็น null ค่า selectedCategoryId จะเป็น 0 (Default)

            //  เรียก LoadProductsToDataGridView พร้อมเงื่อนไขการค้นหา
            LoadProductsToDataGridView(searchTerm, selectedCategoryId);
        }

        private void LoadCategories()
        {
            // Query: ดึง CategoryId และ Name ของประเภทสินค้าทั้งหมด
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

                    // 1. เพิ่มตัวเลือก "ทั้งหมด" (All) ที่มี CategoryId = 0
                    DataRow allRow = dt.NewRow();
                    allRow["CategoryId"] = 0;
                    allRow["Name"] = "--- ทุกประเภทสินค้า ---";
                    dt.Rows.InsertAt(allRow, 0);

                    // 2. ผูก DataTable เข้ากับ ComboBox
                    // NOTE: ต้องมี ComboBox ชื่อ cmbCategoryFilter ใน Designer
                    cmbCategoryFilter.DataSource = dt;
                    cmbCategoryFilter.DisplayMember = "Name";       // สิ่งที่ผู้ใช้เห็น
                    cmbCategoryFilter.ValueMember = "CategoryId";   // ค่า ID ที่ใช้ในการกรอง
                    cmbCategoryFilter.SelectedIndex = 0;            // เลือก "ทั้งหมด" เป็นค่าเริ่มต้น
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("ข้อผิดพลาดในการโหลดประเภทสินค้า:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
