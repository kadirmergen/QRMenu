using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRMenu.Models
{
    public class State
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public byte Id { get; set; }

        [Required]
        [Column(TypeName ="nvarchar(10)")]
        public string Name { get; set; } = "";
    }
}
