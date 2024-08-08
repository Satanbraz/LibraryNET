using System;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class Users
    {
        public int Id { get; set; }
        public int UserRolId { get; set; }
        public string UserRolName { get; set; }
        public string UserName { get; set; }
        public string UserLastName { get; set; }
        public string UserEmail { get; set; }
        public string UserPassword { get; set; }
        public string UserPhone { get; set; }
        public string UserDir { get; set; }
        public string UserRut { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime UserDate { get; set; }

    }
}
