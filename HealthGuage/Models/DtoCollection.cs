using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HealthGuage.Models
{
    public class UserDto
    {
        public string Id { get; set; }
        public string EncId { get; set; }
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public string Profile { get; set; }
        public int Role { get; set; }
    }
    
    public class IngredientDto
    {
        public string? Id { get; set; }
        public string? EncId { get; set; }
        public string? Name { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class ProductDto
    {
        public string? Id { get; set; }
        public string? EncId { get; set; }
        public string? Name { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class PreperationDto
    {
        public string? Id { get; set; }
        public string? EncId { get; set; }
        public string? Name { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class MenuDto
    {
        public string? Id { get; set; }
        public string? EncId { get; set; }
        public string? Name { get; set; }
        public string? Date { get; set; }
        public string? Photo { get; set; }
        public string? Cooking { get; set; }
        public string? Weight { get; set; }
        public string? Link { get; set; }
        public string? PreperationForBeaorStaff { get; set; }
        public List<string>? Ingredients { get; set; }
        public List<string>? Products { get; set; }
        public List<string>? Preperations { get; set; }
        public string? CreatedBy { get; set; }
    }
}
