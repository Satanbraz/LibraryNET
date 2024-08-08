using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class BorrowDetail
    {
        public int Id { get; set; }
        public int BorrowId { get; set; }
        public int BorrowProductId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime BorrowReturnDate { get; set; }
        public DateTime? BorrowReturnedDate { get; set; }
        public bool isActive { get; set; }
    }
}
