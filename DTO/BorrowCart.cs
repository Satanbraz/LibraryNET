using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public  class BorrowCart
    {
        public int Id { get; set; }
        public int IdUser { get; set; }
        public int IdProduct { get; set; }
        public DateTime ItemBorrowDate { get; set; }
        public DateTime ItemBorrowReturnDate { get; set; }
        public Books Books { get; set; }
    }
}
