using System.ComponentModel.DataAnnotations.Schema;

namespace QRMenu.Models
{
    public class RestaurantUser
    {
        public int RestaurantId { get; set; }
        [ForeignKey("RestaurantId")]
        public Restaurant? Restaurant { get; set; }

        public string UserId { get; set; } = "";
        [ForeignKey("UserId")]
        public ApplicationUser? ApplicationUser { get; set; }

        //User Id=SA;Password=reallyStrongPwd123
    }
}
