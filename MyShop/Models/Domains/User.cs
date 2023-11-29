using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyShop.Models.Domains
{
    public class User : IdentityUser
    {
        [Key]
        [ScaffoldColumn(false)]
        public int IdUser { get; set; }

        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(150)]
        public string? Address { get; set; }


        [RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "Your Email is not valid."), MaxLength(100)]

        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Profil { get; set; }

   
        public DateTime DateNaissance { get; set; }

        [MaxLength(250)]
        public string? Password { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }


        // FOREIGN KEYS RELATIONS
        public ICollection<Account>? Accounts { get; set; }

        public ICollection<Bill>? Bills { get; set; }

        public ICollection<Order>? Orders { get; set; }

        public ICollection<Payment>? Payments { get; set; }
    }
}
