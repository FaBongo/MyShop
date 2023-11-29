using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyShop.Models.Domains
{
    
    public class Category
    {
        [Key]
        [ScaffoldColumn(false)]
        public int IdCategory { get; set; }

        [MaxLength(50)]
        public string? CategoryName { get; set; }


        // Propriétés pour la relation parente
        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }

        // Liste des catégories enfants
        public ICollection<Category>? ChildCategories { get; set; }


        // List des produits associés à cette catégorie
        public ICollection<Product>? Products { get; set; }


    }
}
