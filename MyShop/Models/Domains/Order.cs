using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyShop.Models.Domains
{
    public class Order
    {
        [Key]
        [ScaffoldColumn(false)]
        public int IdOrder { get; set; }

        //Foreign key for User
        public int IdUser { get; set; }
        public User User { get; set; }

      //Foreign key for Product
        public int IdProduct { get; set; }
        public Product Product { get; set; }

        public int QuantityProduct { get; set; }

        public int TotalPrice { get; set; }

        public string? Details { get; set; }

        public ICollection<Bill>? Bills { get; set; }
        public ICollection<Payment>? Payments { get; set; }
    }
}
