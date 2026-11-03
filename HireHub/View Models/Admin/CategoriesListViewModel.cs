using HireHub.Models;

namespace HireHub.ViewModels.Admin
{
    public class CategoriesListViewModel
    {
        public List<Category> Categories { get; set; } = new();
    }

    public class CategoryFormModel
    {
        public int CategoryId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.MaxLength(500)]
        public string? Description { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string? Icon { get; set; }

        public bool IsActive { get; set; } = true;
    }
}