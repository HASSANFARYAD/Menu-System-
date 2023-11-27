using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Template.Models;

namespace HealthGuage.Models
{
    public class User : BaseModel
    {

        [Column(TypeName = "nvarchar(255)")]
        public string? Name { get; set; }

        [Column(TypeName = "nvarchar(355)")]
        public string? Email { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Password { get; set; }
        
        [Column(TypeName = "nvarchar(255)")]
        public string? PhoneNumber { get; set; }
        
        [Column(TypeName = "nvarchar(max)")]
        public string? ProfilePicture { get; set; }
        public int? Role { get; set; }

        [NotMapped]
        public IFormFile? File { get; set; }

    }
}
