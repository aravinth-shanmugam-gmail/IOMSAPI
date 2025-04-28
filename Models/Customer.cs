using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOMSAPI.Models
{
    [Table("customer")]
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Addressline { get; set; }

        [MaxLength(255)]
        public string? State { get; set; }

        [MaxLength(255)]
        public string? City { get; set; }

        [MaxLength(255)]
        public string? Country { get; set; }

        [MaxLength(20)]
        public string Zipcode { get; set; }

        [Required]
        [MaxLength(20)]
        public string Phone1 { get; set; }

        [MaxLength(20)]
        public string? Phone2 { get; set; }

        [Required]
        [MaxLength(255)]
        public string Email { get; set; }

        [MaxLength(255)]
        public string PasswordHash { get; set; }
        
        [MaxLength(10)]
        public string OtpCode { get; set; }

        public DateTime? OtpExpiry { get; set; }

        public DateOnly? dob { get; set; }

        public string Status { get; set; }
    }
}
