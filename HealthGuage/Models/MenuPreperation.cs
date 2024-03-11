using System.ComponentModel.DataAnnotations.Schema;
using Template.Models;

namespace HealthGuage.Models
{
    public class MenuPreperation : BaseModel
    {
        [ForeignKey("Menu")]
        public int? MenuId { get; set; }
        public Menu? Menu { get; set; }

        [ForeignKey("Preperation")]
        public int? PreperationId { get; set; }
        public Preperation? Preperation { get; set; }
        public string? Notes { get; set; }
    }
}
