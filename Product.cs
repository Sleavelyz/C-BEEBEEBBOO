using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing; // <--- ต้องเพิ่ม
using System.IO;    // <--- ต้องเพิ่ม

namespace Bee.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImagePath { get; set; }
        // ใช้ int? (Nullable int) เพื่อรองรับค่า NULL จากคอลัมน์ CategoryId ใน DB
        public int? CategoryId { get; set; }

        public string CategoryName { get; set; }

        /// <summary>
        /// Property นี้ใช้สำหรับ DataGridViewImageColumn โดยเฉพาะ 
        /// เพื่อแปลง Path ไฟล์ (string) ให้เป็นวัตถุ Image
        /// </summary>
        public Image ImageDisplay
        {
            get
            {
                if (!string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath))
                {
                    try
                    {
                        // สร้างสำเนาของรูปภาพเพื่อป้องกันไฟล์ถูกล็อคโดย DataGridView
                        using (Image originalImage = Image.FromFile(ImagePath))
                        {
                            return new Bitmap(originalImage);
                        }
                    }
                    catch
                    {
                        // ถ้าโหลดไฟล์ไม่ได้ ให้ส่งคืนค่าว่าง (null)
                        return null;
                    }
                }
                return null;
            }
        }
        // Property สำหรับแสดงสถานะของสินค้า (มี/หมด)
        public string Status
        {
            get
            {
                return StockQuantity > 0 ? "มีสินค้า" : "หมด";
            }
        }
    }
}
