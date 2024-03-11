using Template.Models;

namespace HealthGuage.Models
{
    public class Preperation : BaseModel
    {
        public string? Name { get; set; }
        public List<MenuPreperation?> MenuPreperations { get; set; }
        public string? Notes { get; set; }

    }
}
