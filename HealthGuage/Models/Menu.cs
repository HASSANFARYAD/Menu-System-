using Template.Models;

namespace HealthGuage.Models
{
    public class Menu : BaseModel
    {
        public string? Name { get; set; }
        public DateTime? Date { get; set; }
        public string? Photo { get; set; }
        public string? Cooking { get; set; }
        public string? Weight { get; set; }
        public List<MenuIngredient?> MenuIngredient { get; set; }
        public List<MenuPreperation?> MenuPreperation { get; set; }
        public List<MenuProduct?> MenuProduct { get; set; }
    }
}
