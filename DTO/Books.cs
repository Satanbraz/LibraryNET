using System;
using System.Collections.Generic;

namespace DTO
{
    public class Books
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int BookYear { get; set; }
        public int BookGenreId { get; set; }
        public string BookGenre { get; set; }
        public int BookStateId { get; set; }
        public string BookState { get; set; }
        public int BookEditId { get; set; }
        public string BookEdit { get; set; }
        public string BookBC { get; set; }
        public int BookPrice { get; set; }
        public int Stock { get; set; }
        public byte[] BookImage { get; set; }

    }
}
