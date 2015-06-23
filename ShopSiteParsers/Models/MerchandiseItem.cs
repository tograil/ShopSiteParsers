
namespace ShopSiteParsers.Models
{
    public class MerchandiseItem
    {

        public class Availability
        {
            public string Color { get; set; }
            public string Size { get; set; }
            public string Quantity { get; set; }
        }

        public string Category { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public Availability Avail { get; set; }
        public string Image { get; set; }
        public string Consist { get; set; }

    }
}
