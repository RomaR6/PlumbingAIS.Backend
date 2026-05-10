using System.ComponentModel.DataAnnotations.Schema;

namespace PlumbingAIS.Backend.Models
{
    public enum UserRoleType
    {
        Admin,
        Manager,
        User
    }

    public class Role : DictionaryEntity
    {
        [NotMapped]
        public UserRoleType RoleType { get; set; }

        [NotMapped]
        public string Description { get; set; } = string.Empty;

        public bool IsAdministrative()
        {
            return Name == "Admin" || Name == "Manager";
        }

        public string GetRolePermissionsSummary() => $"Permissions for {Name} are default.";
    }
}