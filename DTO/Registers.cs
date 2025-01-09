using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class Registers
    {
        public string TrancId {  get; set; }
        public int TrancUserId { get; set; }
        public string TrancUserName { get; set; }
        public int TrancType { get; set; }
        public DateTime TrancDate { get; set; }
        public int TrancAmount { get; set; }
    }
}
