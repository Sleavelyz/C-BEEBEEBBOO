using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Bee.Models;

namespace Bee
{
    public partial class PaymentForm : Form
    {
        public PaymentForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        public string ShippingAddress { get; private set; } 
        public string PaymentSlipPath { get; private set; } 

        private string _slipImagePath; // ตัวแปรสำหรับเก็บที่อยู่ไฟล์สลิป

        private void PaymentForm_Load(object sender, EventArgs e)
        {

        }

        private void txtAddress_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnUploadSlip_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                // กรองเฉพาะไฟล์รูปภาพที่ยอมรับ
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png";
                ofd.Title = "แนบสลิปโอนเงิน";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // 1. บันทึกที่อยู่ไฟล์
                        _slipImagePath = ofd.FileName;

                        // 2. แสดงรูปภาพสลิปใน PictureBox
                        // NOTE: ต้องมี using System.IO; และ using System.Drawing; ที่ด้านบน
                        pbSlipPreview.Image = Image.FromFile(_slipImagePath);
                        pbSlipPreview.SizeMode = PictureBoxSizeMode.Zoom;

                        MessageBox.Show("แนบสลิปสำเร็จ", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ข้อผิดพลาดในการโหลดรูปภาพสลิป: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnSubmitPayment_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            // 1. บันทึกค่าลงใน Public Properties
            ShippingAddress = txtAddress.Text; // (สมมติชื่อ txtAddress)
            PaymentSlipPath = _slipImagePath; // (Path สลิปที่ได้จากการอัปโหลด)

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void pbSlipPreview_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// ตรวจสอบว่ามีการกรอกที่อยู่และแนบสลิปแล้ว
        /// </summary>
        private bool ValidateInput()
        {
            // NOTE: txtAddress คือ TextBox ที่ใช้กรอกที่อยู่
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("กรุณากรอกที่อยู่สำหรับจัดส่งสินค้า", "ข้อมูลไม่ครบถ้วน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // NOTE: _slipImagePath จะถูกกำหนดค่าเมื่อไฟล์ถูกเลือกใน btnUploadSlip_Click
            if (string.IsNullOrWhiteSpace(_slipImagePath))
            {
                MessageBox.Show("กรุณาแนบสลิปโอนเงินเพื่อยืนยันการชำระเงิน", "ข้อมูลไม่ครบถ้วน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void guna2HtmlLabel1_Click(object sender, EventArgs e)
        {

        }
    }
}
