using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using IOMSAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IOMSAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecurityController : ControllerBase
    {
        private readonly IOMSContext _context;

        public SecurityController(IOMSContext context)
        {
            _context = context;
        }

        [HttpPost("Register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            if (_context.Customers.Any(c => c.Email == request.Email || c.Phone1 == request.Phone1))
            {
                return BadRequest("Email or phone number already registered.");
            }

            var otpCode = GenerateOtpCode();
            var customer = new Customer
            {
                Name = request.Name,
                Addressline = request.Addressline,
                State = request.State,
                City = request.City,
                Country = request.Country,
                Zipcode = request.Zipcode,
                Phone1 = request.Phone1,
                Phone2 = request.Phone2,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                OtpCode = otpCode,
                OtpExpiry = DateTime.UtcNow.AddMinutes(10), // OTP valid for 10 minutes
                Status = "PENDING" // Set initial status to PENDING
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Send OTP to email
            await SendOtpEmail(request.Email, otpCode);

            return Ok("Registration successful. Please check your email for the OTP.");
        }

        [HttpPost("VerifyOtp")]
        public async Task<ActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Email == request.Email);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            if (customer.OtpCode != request.OtpCode || customer.OtpExpiry < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired OTP.");
            }

            // Clear OTP fields after successful verification
            customer.OtpCode = null;
            customer.OtpExpiry = null;
            customer.Status = "COMPLETED";
            await _context.SaveChangesAsync();

            return Ok("OTP verified successfully. Registration complete.");
        }

        [HttpPost("Login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Email == request.Email);
            if (customer == null || !VerifyPassword(request.Password, customer.PasswordHash))
            {
                return Unauthorized("Invalid email or password.");
            }

            // Generate and return a token (JWT or similar) for authenticated sessions
            var token = GenerateToken(customer);
            return Ok(new { Token = token });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }

        private string GenerateOtpCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private async Task SendOtpEmail(string email, string otpCode)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential("kayalininaturals@gmail.com", "dvhdukiiwxosngxb"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("kayalininaturals@gmail.com"),
                Subject = "Your OTP Code",
                Body = $"Your OTP code is {otpCode}",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }

        private string GenerateToken(Customer customer)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key_here"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, customer.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "https://yourapi.com", // The issuer of the token
                audience: "https://yourclientapp.com", // The intended audience of the token
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public class RegisterRequest
        {
            public string Name { get; set; }
            public string Addressline { get; set; }
            public string State { get; set; }
            public string City { get; set; }
            public string Country { get; set; }
            public string Zipcode { get; set; }
            public string Phone1 { get; set; }
            public string Phone2 { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class VerifyOtpRequest
        {
            public string Email { get; set; }
            public string OtpCode { get; set; }
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}
