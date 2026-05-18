using BCrypt.Net;
using BenhVien_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BenhVien_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly QlBenhvienContext _context;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AuthController> _logger;

        private const int OtpTtlMinutes = 5;

        public AuthController(
            QlBenhvienContext context,
            IConfiguration config,
            IMemoryCache cache,
            ILogger<AuthController> logger)
        {
            _context = context;
            _config = config;
            _cache = cache;
            _logger = logger;
        }

        // 1. GỬI OTP
        [HttpPost("send-otp")]
        public IActionResult SendOtp([FromBody] PhoneRequest model)
        {
            if (model is null || string.IsNullOrWhiteSpace(model.Phone))
                return BadRequest(new { message = "Vui lòng nhập số điện thoại" });

            // Generate secure 6-digit OTP
            int otpInt = RandomNumberGenerator.GetInt32(100000, 1000000);
            string otp = otpInt.ToString();

            // Store in memory cache with absolute expiration
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(OtpTtlMinutes)
            };

            _cache.Set(model.Phone, otp, cacheEntryOptions);

            _logger.LogInformation("OTP generated for phone {Phone}", model.Phone);

            // For demo we return otp; in production DO NOT return OTP in API response.
            return Ok(new { message = "OTP đã gửi (demo)", phone = model.Phone, otp });
        }

        // 2. XÁC THỰC OTP
        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequest model)
        {
            if (model is null || string.IsNullOrWhiteSpace(model.Phone) || string.IsNullOrWhiteSpace(model.Otp))
                return BadRequest(new { message = "Thiếu dữ liệu" });

            if (!_cache.TryGetValue<string>(model.Phone, out var storedOtp) ||
                storedOtp != model.Otp)
            {
                return BadRequest(new { message = "OTP không hợp lệ hoặc đã hết hạn" });
            }

            // Remove OTP after successful verification
            _cache.Remove(model.Phone);
            return Ok(new { message = "OTP hợp lệ", phone = model.Phone });
        }

        // 3. ĐĂNG KÝ
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterPhoneModel model)
        {
            if (model is null)
                return BadRequest(new { message = "Thiếu dữ liệu" });

            if (string.IsNullOrWhiteSpace(model.Phone) ||
                string.IsNullOrWhiteSpace(model.Password) ||
                string.IsNullOrWhiteSpace(model.FullName))
            {
                return BadRequest(new { message = "Thiếu dữ liệu bắt buộc" });
            }

            bool existed = await _context.Users
                .AnyAsync(x => x.Phone == model.Phone);

            if (existed)
            {
                return BadRequest(new { message = "Số điện thoại đã tồn tại" });
            }

            string roleCode = (model.Role ?? "P").ToUpperInvariant();
            string passwordPattern = @"^(?=.*[A-Z])(?=.*[\W_]).{8,}$";

            if (!Regex.IsMatch(model.Password, passwordPattern))
            {
                return BadRequest(new
                {
                    message =
                    "Mật khẩu phải có ít nhất 8 ký tự, 1 chữ in hoa và 1 ký tự đặc biệt"
                });
            }

            var user = new User
            {
                Phone = model.Phone.Trim(),
                PassWord = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = roleCode,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (roleCode == "P")
            {
                var patient = new Patient
                {
                    FullName = model.FullName,
                    Phone = model.Phone.Trim(),
                    UserId = user.UserId
                };
                _context.Patients.Add(patient);
            }
            else if (roleCode == "D")
            {
                var doctor = new Doctor
                {
                    FullName = model.FullName,
                    UserId = user.UserId
                };
                _context.Doctors.Add(doctor);
            }

            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Đăng ký thành công",
                token,
                role = user.Role,
                fullName = model.FullName
            });
        }

        // 4. ĐĂNG NHẬP
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginPhoneModel model)
        {
            if (model is null || string.IsNullOrWhiteSpace(model.Phone) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { message = "Thiếu dữ liệu" });

            var user = await _context.Users
                .Include(x => x.Patients)
                .Include(x => x.Doctors)
                .FirstOrDefaultAsync(
                    x => x.Phone.Trim() == model.Phone.Trim()
                    );

            if (user == null)
            {
                return Unauthorized(new { message = "Số điện thoại không tồn tại" });
            }

            bool checkPassword = BCrypt.Net.BCrypt.Verify(model.Password, user.PassWord);

            if (!checkPassword)
            {
                return Unauthorized(new { message = "Sai mật khẩu" });
            }

            string fullName = string.Empty;
            if (user.Role == "P")
                fullName = user.Patients?.FirstOrDefault()?.FullName ?? string.Empty;
            else if (user.Role == "D")
                fullName = user.Doctors?.FirstOrDefault()?.FullName ?? string.Empty;

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Đăng nhập thành công",
                token,
                userId = user.UserId,
                role = user.Role,
                fullName
            });
        }

        private string GenerateJwtToken(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            string key = _config["Jwt:Key"];
            string issuer = _config["Jwt:Issuer"];
            string audience = _config["Jwt:Audience"];
            string expireStr = _config["Jwt:ExpireMinutes"];

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(expireStr))
            {
                _logger.LogError("JWT configuration missing");
                throw new InvalidOperationException("JWT configuration is missing");
            }

            if (!double.TryParse(expireStr, out double expireMinutes))
            {
                expireMinutes = 60; // default
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.MobilePhone, user.Phone ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //Change passwword
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordModel model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            string passwordPattern = @"^(?=.*[A-Z])(?=.*[\W_]).{8,}$";

            if (!Regex.IsMatch(model.NewPassword, passwordPattern))
            {
                return BadRequest(new
                {
                    message =
                    "Mật khẩu phải có ít nhất 8 ký tự, 1 chữ in hoa và 1 ký tự đặc biệt"
                });
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));

            if (user == null)
            {
                return NotFound();
            }

            bool checkPassword = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PassWord);

            if (!checkPassword)
            {
                return BadRequest(new
                {
                    message = "Mật khẩu hiện tại không đúng"
                });
            }

            user.PassWord = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đổi mật khẩu thành công"
            });
        }


    }

    // Models / DTOs (moved inside namespace)
    public class PhoneRequest
    {
        public string Phone { get; set; } = string.Empty;
    }

    public class VerifyOtpRequest
    {
        public string Phone { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }

    public class RegisterPhoneModel
    {
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Role { get; set; }
    }

    public class LoginPhoneModel
    {
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class ChangePasswordModel
    {
        public string CurrentPassword { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;
    }
}