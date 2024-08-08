using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class Sell
    {
        public int Id { get; set; }
        public int IdUser { get; set; }
        public int IdClient { get; set; }
        public string RutClient { get; set; }
        public string IdTransaction { get; set; }
        public string PhoneUser { get; set; }
        public string DirUser { get; set; }
        public int ProductsQ { get; set; }
        public int TotalBruto { get; set; }
        public int IVA { get; set; }
        public int TotalAmount { get; set; }
        public DateTime SellDate { get; set; }

        public SellDetail sellDetail { get; set; }
        public Books books { get; set; }
        public List<Books> bookList { get; set; }
        public int SellCount { get; set; }
    }
}
