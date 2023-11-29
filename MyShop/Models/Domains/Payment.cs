using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyShop.Models.Domains
{
    public class Payment
    {
        [Key]
        [ScaffoldColumn(false)]
        public int IdPayment { get; set; }

        //Foreign key for Standard
        public int IdUser { get; set; }
        public User User { get; set; }

        public int IdOrder { get; set; }
        public Order Order { get; set; }

        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        [MaxLength(250)]
        public string? Details { get; set; }
    }
}
