using System.ComponentModel.DataAnnotations;

namespace security.Models
{
    public class User
    {
        [Key]
        public Guid IdUser { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set;}

        public string Password { get; set;}

        public string UserRole { get; set;}

    }
}
