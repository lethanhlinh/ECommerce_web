using Microsoft.AspNetCore.Identity;

namespace ECommerce_web.Models
{
    public class AppUserModel : IdentityUser
    {
        public string Occupayion {  get; set; }
        public string RoleId { get; set; }
    }
}
