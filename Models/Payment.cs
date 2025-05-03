using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOMSAPI.Models
{
    [Table("Payment")]
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public string RazorpayOrderId { get; set; }

        public string RazorpayPaymentId { get; set; }

        public string RazorpaySignature { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; }

        [Required]
        public string Status { get; set; } // CREATED, CONFIRMED, FAILED

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? ConfirmedAt { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }
    }
}