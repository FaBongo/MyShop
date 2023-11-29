using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyShop.Models.Domains
{
    public class Bill
    {
        [Key]
        [ScaffoldColumn(false)]
        public int IdBill { get; set; }

        //Foreign key for Standard
     
        public int IdUser { get; set; }
        public User User { get; set; }

       
        public int IdOrder { get; set; }
        public Order Order { get; set; }

        public int Price { get; set; }
    }
}
