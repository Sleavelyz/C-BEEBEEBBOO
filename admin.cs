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
    public partial class admin : Form
    {
        public admin()
        {
            InitializeComponent();
            // กำหนดให้หน้า Dashboard แสดงขึ้นมาเป็นหน้าแรก
            LoadUserControl(new UCDashboard()); 
        }

        //  ใช้สำหรับโหลด UserControl ลงใน Panel ***
        private void LoadUserControl(UserControl userControl)
        {
            //  ลบ Control เก่าทั้งหมดออกจาก Panel
            contentPanel.Controls.Clear();

            //  ตั้งค่า UserControl ใหม่ให้เติมเต็มพื้นที่ Panel
            userControl.Dock = DockStyle.Fill;

            //  เพิ่ม UserControl เข้าไปใน Panel
            contentPanel.Controls.Add(userControl);
        }

        private void productbt_Click(object sender, EventArgs e)
        {
            // สร้างอินสแตนซ์ใหม่ของ Product Management UserControl แล้วโหลด
            LoadUserControl(new UCProductManagement());
        }

       

        private void userbt_Click(object sender, EventArgs e)
        {
            // สร้างอินสแตนซ์ใหม่ของ User Management UserControl แล้วโหลด
            LoadUserControl(new UCUserManagement());
        }

        private void adminbg_Paint(object sender, PaintEventArgs e)
        {

        }

        private void contentPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dashboardbt_Click(object sender, EventArgs e)
        {
            // สร้างอินสแตนซ์ใหม่ของ Dashboard UserControl แล้วโหลด
            LoadUserControl(new UCDashboard());
        }

        private void logoutbt_Click(object sender, EventArgs e)
        {
            // ยืนยันการออกจากระบบ
            DialogResult result = MessageBox.Show("คุณแน่ใจหรือไม่ว่าต้องการออกจากระบบ?", "ยืนยันการออกจากระบบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                //  สร้างอินสแตนซ์ของหน้า Login ใหม่
                login loginForm = new login();

                //  ซ่อน/ปิดฟอร์ม Admin ปัจจุบัน
                this.Hide();
                // NOTE: ใช้ this.Close() ก็ได้ ถ้าคุณต้องการปิดฟอร์ม Admin อย่างถาวร

                // แสดงฟอร์ม Login
                loginForm.Show();
            }
        }

        private void admin_Load(object sender, EventArgs e)
        {

        }

        private void ordersbt_Click(object sender, EventArgs e)
        {
            // สร้างอินสแตนซ์ใหม่ของ User Control จัดการคำสั่งซื้อแล้วโหลด
            LoadUserControl(new UCManageOrders());
        }

        private void historybt_Click(object sender, EventArgs e)
        {
            LoadUserControl(new HistoryUC());
        }
    }
}
