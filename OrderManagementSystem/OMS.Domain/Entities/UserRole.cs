using System.ComponentModel.DataAnnotations.Schema;

namespace OMS.Domain.Entities
{
    [Table("UserRoles")]
    public class UserRole
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }

        [ForeignKey("UserId")]
        public Customer User { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }
    }
}