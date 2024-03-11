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
    
    public class GeneralNameDto
    {
        public string? Name { get; set; }
    }    
    
    public class IngredientDto
    {
        public string? Id { get; set; }
        public string? EncId { get; set; }
        public string? Name { get; set; }
        public string? Notes { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class MenuCategoryDto
    {
        public string? Id { get; set; }
        public string? EncId { get; set; }
        public string? Name { get; set; }
        public string? Notes { get; set; }
        public string? ProfilePath { get; set; }
        public string? CreatedBy { get; set; }
    }
    public class MenuTypeDto
    {
        public string? Id { get; set; }
        public string? EncId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class ProductDto
    {
        public string? Id { get; set; }
        public string? EncId { get; set; }
        public string? Name { get; set; }
        public string? Notes { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class PreperationDto
    {
        public string? Id { get; set; }
        public string? EncId { get; set; }
        public string? Name { get; set; }
        public string? Notes { get; set; }

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
        public string? Ingredients { get; set; }
        public string? Products { get; set; }
        public string? Preperations { get; set; }
        public string? CreatedBy { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryId { get; set; }
        public string? CategoryPhoto { get; set; }
        public string? Qunantity { get; set; }
        public string? Notes { get; set; }

    }

    public class AddMenuDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Date { get; set; }
        public string? Photo { get; set; }
        public string? Cooking { get; set; }
        public string? Weight { get; set; }
        public string? Link { get; set; }
        public string? PreperationForBeaorStaff { get; set; }
        public string[]? Ingredients { get; set; }
        public string[]? Products { get; set; }
        public string[]? Preperations { get; set; }
        public IFormFile? Picture { get; set; }
        public string? CategoryId { get; set; }
        public string? Description {  get; set; }
        public string? MenuTypeId {  get; set; }
        public string? Notes { get; set; }

    }


    public class AddMenuCategoryDto
    {
        public int? Id { get; set;}
        public string? Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? Picture { get; set;}
        public string? Notes { get; set; }

    }

    public class AddMenuTypeDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
