using DocumentFormat.OpenXml.ExtendedProperties;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using HealthGuage.Models;

namespace Template.Models
{
    public class ContentFile
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255)]
        [Column(TypeName = "nvarchar(255)")]
        public string? FileName { get; set; }

        [StringLength(355)]
        [Column(TypeName = "nvarchar(355)")]
        public string? FilePath { get; set; }

        [StringLength(355)]
        [Column(TypeName = "nvarchar(355)")]
        public string? FileExtension { get; set; }

        public double? FileSize { get; set; }

        [StringLength(150)]
        [Column(TypeName = "nvarchar(150)")]
        public string? Type { get; set; }   // e.g "Profile" or  "Tile" etc
        public int? UploadedBy { get; set; }
        public int IsActive { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DeletedAt { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; }
        public User? User { get; set; }

    }
    
}
