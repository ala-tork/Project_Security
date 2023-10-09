using System.ComponentModel.DataAnnotations;

namespace security.Models
{
    public class Reclamation
    {
        [Key]
        public int IdReclamation { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }

        public string Message { get; set; }

        public Guid IdUser { get; set; }
    }
}
