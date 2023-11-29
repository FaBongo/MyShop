using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyShop.Models.Domains
{
    public class Product
    {
        [Key]
        [ScaffoldColumn(false)]
        public int IdProduct { get; set; }

        [MaxLength(50)]
        public string? NameProduct { get; set; }

        public int StockQuantity { get; set; }

        public int UnitPrice { get; set; }

        public DateTime RegistrationDate { get; set; }


        //Foreign key for Standard
        public int IdCategorie { get; set; }
        public Category? Category { get; set; }

        public ICollection<Order>? Orders { get; set; }

        

    }
}
