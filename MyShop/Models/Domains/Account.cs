using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyShop.Models.Domains
{
    public class Account
    {
        [Key]
        [ScaffoldColumn(false)]
        public int IdAccount { get; set; }

        //Foreign key for User
        public int IdUser { get; set; }
        public User User { get; set; }


        [MaxLength(250)]
        public string? Details { get; set; }
    }
}
