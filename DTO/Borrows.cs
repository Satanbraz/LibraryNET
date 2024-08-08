using System;

namespace DTO
{
    public class Borrows
    {
        public int Id { get; set; }
        public int IdUser { get; set; }
        public int IdClient { get; set; }
        public string IdTransaction { get; set; }
        public string PhoneUser { get; set; }
        public string DirUser { get; set; }
        public string UserRut { get; set; }
        public int ProductsQ { get; set; }
        public DateTime BorrowRegisterDate { get; set; }
        public BorrowDetail borrowDetail { get; set; }
        public Books books { get; set; }
    }
}
