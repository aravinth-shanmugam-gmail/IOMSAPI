using IOMSAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Razorpay.Api;
using Razorpay.Api.Errors;

namespace IOMSAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IOMSContext _context;
        private const string RazorpayKey = "rzp_live_bGRSICbuiYgtfO";//testkey"rzp_test_TaMeeqP3h7Cm71";
        private const string RazorpaySecret = "dzefmrjSBSakgycsh4XUgRBa";//testsecret"h3kp1K28hiqiWvRuFL8WMuAi";

        public PaymentController(IOMSContext context)
        {
            _context = context;
        }

        [HttpPost("CreateOrder")]
        [Authorize] // Ensure the user is authenticated
        public async Task<ActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            // Get the customer ID from the JWT token
            var customerId = int.Parse(User.Claims.First(c => c.Type == "CustomerId").Value);

            // Retrieve the product (InventoryItem) from the database
            var product = await _context.InventoryItems.FirstOrDefaultAsync(i => i.Id == request.ProductId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            // Calculate the total price
            var totalPrice = product.PricePerUnit * request.Quantity;

            try
            {
                // Initialize Razorpay client
                var client = new RazorpayClient(RazorpayKey, RazorpaySecret);

                // Create order payload
                var options = new Dictionary<string, object>
                {
                    { "amount", (int)(totalPrice * 100) }, // Amount in paise (e.g., ₹1 = 100 paise)
                    { "currency", "INR" },
                    { "receipt", $"order_rcpt_1" }
                };

                // Create the order
                var order = client.Order.Create(options);

                // Save the payment details in the database
                var payment = new Models.Payment
                {
                    CustomerId = customerId,
                    RazorpayOrderId = order["id"].ToString(),
                    Amount = totalPrice,
                    Currency = "INR",
                    Status = "CREATED",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Return the order details to the UI
                return Ok(new
                {
                    OrderId = order["id"].ToString(),
                    Amount = (int)(totalPrice * 100),
                    Currency = "INR"
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to create Razorpay order: {ex.Message}");
            }
        }


        // Define a class to map the Razorpay order response
        public class RazorpayOrderResponse
        {
            public string Id { get; set; }
            public string Entity { get; set; }
            public int Amount { get; set; }
            public string Currency { get; set; }
            public string Receipt { get; set; }
            public string Status { get; set; }
        }



        [HttpPost("ConfirmPayment")]
        [Authorize] // Ensure the user is authenticated
        public async Task<ActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            // Get the customer ID from the JWT token
            var customerId = int.Parse(User.Claims.First(c => c.Type == "CustomerId").Value);

            // Retrieve the payment record from the database
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.RazorpayOrderId == request.OrderId && p.CustomerId == customerId);

            if (payment == null)
            {
                return NotFound("Payment record not found.");
            }

            try
            {
                // Initialize Razorpay client
                var client = new RazorpayClient(RazorpayKey, RazorpaySecret);

                // Verify the payment signature using Razorpay SDK
                var attributes = new Dictionary<string, string>
        {
            { "razorpay_order_id", request.OrderId },
            { "razorpay_payment_id", request.PaymentId },
            { "razorpay_signature", request.Signature }
        };

                // This will throw an exception if the signature is invalid
                Utils.verifyPaymentSignature(attributes);

                // Update the payment status in the database
                payment.RazorpayPaymentId = request.PaymentId;
                payment.RazorpaySignature = request.Signature;
                payment.Status = "CONFIRMED";
                payment.ConfirmedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok("Payment confirmed successfully.");
            }
            catch (SignatureVerificationError)
            {
                // Update the payment status to FAILED in case of an invalid signature
                payment.Status = "FAILED";
                await _context.SaveChangesAsync();
                return BadRequest("Invalid payment signature.");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while confirming the payment: {ex.Message}");
            }
        }


        private bool VerifySignature(string orderId, string paymentId, string signature)
        {
            var payload = $"{orderId}|{paymentId}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(RazorpaySecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var generatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
            return generatedSignature == signature;
        }

    }

    public class CreateOrderRequest
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class ConfirmPaymentRequest
    {
        public string OrderId { get; set; }
        public string PaymentId { get; set; }
        public string Signature { get; set; }
    }


}
