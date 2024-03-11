using System.ComponentModel.DataAnnotations.Schema;
using Template.Models;

namespace HealthGuage.Models
{
    public class MenuProduct : BaseModel
    {
        [ForeignKey("Menu")]
        public int? MenuId { get; set; }
        public Menu? Menu { get; set; }

        [ForeignKey("Product")]
        public int? ProductId { get; set; }
        public Product? Product { get; set; }
        public string? Notes { get; set; }
    }
}
