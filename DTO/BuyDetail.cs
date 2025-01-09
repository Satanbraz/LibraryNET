using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class BuyDetail
    {
        public int Id { get; set; }
        public int BuyId { get; set; }
        public int BuyProductId { get; set; }
        public int BuyBookQ {  get; set; }
        public int BookPrice { get; set; }
        public int BuyTotal { get; set; }

    }
}
