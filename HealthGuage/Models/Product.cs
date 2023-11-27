using Template.Models;

namespace HealthGuage.Models
{
    public class Product : BaseModel
    {
        public string? Name { get; set; }
        public List<MenuProduct?> MenuProducts { get; set; }

    }
}
