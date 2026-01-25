using System.ComponentModel.DataAnnotations;

namespace peluqueria.Models
{
    public class UsersModel
    {
        [Key]
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Email { get; set; }

        public string? Password { get; set; }

    }
}