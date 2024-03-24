using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRMenu.Models
{
    public class Restaurant
    {
        public int Id { get; set; }

        [StringLength(100,MinimumLength =2)]
        [Column(TypeName = "nvarchar(100)")]
        public string BranchName { get; set; } = "";

        [Phone]
        [Column(TypeName = "varchar(30)")]
        public string PhoneNumber { get; set; } = "";

        [Column(TypeName = "char(5)")]
        [DataType(DataType.PostalCode)]
        public string PostalCode { get; set; } = "";

        [StringLength(200, MinimumLength = 5)]
        [Column(TypeName = "nvarchar(200)")]
        public string Address { get; set; } = "";

        public DateTime RegisterDate { get; set; }

        public byte StateId { get; set; }
        [ForeignKey("StateId")]
        public State? State { get; set; }

        public int CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }

    }
}
