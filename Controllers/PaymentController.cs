using IOMSAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{RazorpayKey}:{RazorpaySecret}")));

            var payload = new
            {
                amount = (int)(totalPrice * 100), // Amount in paise (e.g., ₹1 = 100 paise)
                currency = "INR",
                receipt = $"order_rcpt_{Guid.NewGuid()}"
                //,
                //payment_capture = 1 // Auto-capture payment
            };

            var response = await client.PostAsJsonAsync("https://api.razorpay.com/v1/orders", payload);
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Failed to create Razorpay order.");
            }

            // Deserialize the response using System.Text.Json
            var responseData = await response.Content.ReadFromJsonAsync<RazorpayOrderResponse>();
            if (responseData == null || string.IsNullOrEmpty(responseData.Id))
            {
                return BadRequest("Invalid response from Razorpay.");
            }

            // Save the payment details in the database
            var payment = new Payment
            {
                CustomerId = customerId,
                RazorpayOrderId = responseData.Id,
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
                OrderId = responseData.Id,
                Amount = totalPrice,
                Currency = "INR"
            });
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

            // Verify the payment signature
            var isValidSignature = VerifySignature(request.OrderId, request.PaymentId, request.Signature);
            if (!isValidSignature)
            {
                // Update the payment status to FAILED in case of an invalid signature
                payment.Status = "FAILED";
                await _context.SaveChangesAsync();
                return BadRequest("Invalid payment signature.");
            }

            // Update the payment status in the database
            payment.RazorpayPaymentId = request.PaymentId;
            payment.RazorpaySignature = request.Signature;
            payment.Status = "CONFIRMED";
            payment.ConfirmedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("Payment confirmed successfully.");
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
