using Bee.Models;
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
    public partial class UCProductCard : UserControl
    {
        //Event Delegate สำหรับส่งสินค้าไปที่ mainpage
        public event Action<Product, int> OnAddToCart;
        private Product _product;
        private int _availableStock; // เพิ่มตัวแปรนี้เพื่อเก็บสต็อกที่เหลือจริง
        public UCProductCard(Product product, int availableStock)
        {
            InitializeComponent();
            _product = product;
            _availableStock = availableStock; // เก็บค่าสต็อกที่เหลือ
            DisplayProductInfo();
        }

        private void UCProductCard_Load(object sender, EventArgs e)
        {

        }

        private void DisplayProductInfo()
        {
            // ตรวจสอบว่า Controls ถูกตั้งชื่อถูกต้องใน Designer
            lblName.Text = _product.Name;
            lblDescription.Text = _product.Description;
            lblPrice.Text = "฿" + _product.Price.ToString("#,##0.00");

            // โหลดรูปภาพ (ต้องมี using System.IO; ที่ด้านบน)
            if (!string.IsNullOrEmpty(_product.ImagePath) && System.IO.File.Exists(_product.ImagePath))
            {
                pbImage.Image = Image.FromFile(_product.ImagePath);
                pbImage.SizeMode = PictureBoxSizeMode.Zoom;
            }

            //  ใช้การบังคับแปลงชนิด (Casting) เพื่อเข้าถึงคุณสมบัติ Guna2NumericUpDown
            Guna.UI2.WinForms.Guna2NumericUpDown nudQuantity = (Guna.UI2.WinForms.Guna2NumericUpDown)txtQuantity;

            // ตั้งค่า Maximum และ Minimum ตามสต็อกที่เหลือ
            nudQuantity.Minimum = 1;
            nudQuantity.Maximum = (_availableStock > 0) ? _availableStock : 1;
            nudQuantity.Value = 1;


            // ตรวจสอบสถานะปุ่ม: ถ้าสต็อกสั่งได้เหลือ 0
            if (_availableStock <= 0)
            {
                btnAddToCart.Text = "สินค้าหมด"; // ✅ เปลี่ยนคำว่า "เพิ่มลงตะกร้า"
                btnAddToCart.Enabled = false;
                nudQuantity.Enabled = false;
            }
            else
            {
                btnAddToCart.Text = "เพิ่มลงตะกร้า";
                btnAddToCart.Enabled = true;
                nudQuantity.Enabled = true;
            }
        }

        private void btnAddToCart_Click(object sender, EventArgs e)
        {
            int quantity = 0;

            //  อ่านค่าจาก Control 
            if (txtQuantity is Guna.UI2.WinForms.Guna2NumericUpDown nudQuantity)
            {
                quantity = (int)nudQuantity.Value;
            }
            

            //  เรียกใช้ Event และส่งข้อมูล (ส่งสัญญาณไปให้ mainpage จัดการต่อ)
            if (quantity > 0)
            {
                OnAddToCart?.Invoke(_product, quantity);
            }
        }

        private void pbImage_Click(object sender, EventArgs e)
        {

        }

        private void lblName_Click(object sender, EventArgs e)
        {

        }

        private void lblDescription_Click(object sender, EventArgs e)
        {

        }

        private void lblPrice_Click(object sender, EventArgs e)
        {

        }

        private void txtQuantity_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
