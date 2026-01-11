using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bee.Models
{
    public class User
    {
        // Properties ต้องตรงกับชื่อคอลัมน์ในตาราง 'users'
        public int id { get; set; } // Primary Key
        public string username { get; set; }
        public string email { get; set; }
        public string role { get; set; } // บทบาท (admin, user)
    }
}
