using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class SellDetail
    {
        public int Id { get; set; }
        public int SellId { get; set; }
        public int SellProductId { get; set; }
        public int SellBookQ { get; set; }
        public int BookPrice { get; set; }
        public int SellTotal { get; set; }
    }
}
