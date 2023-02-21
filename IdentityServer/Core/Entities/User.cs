using IdentityServer.Core.Enum;

namespace IdentityServer.Core.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        public UserRole Role { get; set; }
    }
}
