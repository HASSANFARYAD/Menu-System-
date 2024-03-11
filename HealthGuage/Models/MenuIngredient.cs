using System.ComponentModel.DataAnnotations.Schema;
using Template.Models;

namespace HealthGuage.Models
{
    public class MenuIngredient :BaseModel
    {
        [ForeignKey("Menu")]
        public int? MenuId { get; set; }
        public Menu? Menu { get; set; }

        [ForeignKey("Ingredient")]
        public int? IngredientId { get; set; }
        public Ingredient? Ingredient { get; set; }
        public string? Notes { get; set; }
    }

}
