using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthController(RaceDayContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // POST: api/auth/register
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "First name, last name, email and password are required."
                });
            }

            if (request.Password.Length < 8)
            {
                return BadRequest(new
                {
                    message = "Password must contain at least 8 characters."
                });
            }

            var email = request.Email.Trim();
            // Basic email format validation.
            try
            {
                new System.Net.Mail.MailAddress(email);
            }
            catch
            {
                return BadRequest(new { message = "Invalid email address." });
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                return Conflict(new
                {
                    message = "Email is already registered."
                });
            }

            // Public registration cannot assign privileged roles.
            const string role = "Participant";

            User user = new()
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                Phone = request.Phone?.Trim(),
                Role = role
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Created("/api/auth/login", new
            {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone,
                user.Role
            });
        }

        // POST: api/auth/create-organiser
        // Only existing organisers may create other organisers.
        [Authorize(Roles = "Organiser")]
        [HttpPost("create-organiser")]
        public async Task<IActionResult> CreateOrganiser(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "First name, last name, email and password are required."
                });
            }

            if (request.Password.Length < 8)
            {
                return BadRequest(new
                {
                    message = "Password must contain at least 8 characters."
                });
            }

            var email = request.Email.Trim();
            try
            {
                new System.Net.Mail.MailAddress(email);
            }
            catch
            {
                return BadRequest(new { message = "Invalid email address." });
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                return Conflict(new
                {
                    message = "Email is already registered."
                });
            }

            var user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                Phone = request.Phone?.Trim(),
                Role = "Organiser"
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Created("/api/auth/login", new
            {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone,
                user.Role
            });
        }

        // POST: api/auth/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Email and password are required."
                });
            }

            var email = request.Email.Trim();

            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var verification = _passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, request.Password);

            if (verification == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            List<Claim> claims = new()
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserID.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    $"{user.FirstName} {user.LastName}"),

                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            ClaimsIdentity identity = new(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            ClaimsPrincipal principal = new(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return Ok(new
            {
                message = "Login successful.",
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role
            });
        }

        // POST: api/auth/logout
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok(new
            {
                message = "Logout successful."
            });
        }

        // GET: api/auth/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out var id))
                return Unauthorized();

            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserID == id)
                .Select(u => new
                {
                    u.UserID,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Phone,
                    u.Role
                })
                .SingleOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // PUT: api/auth/me
        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe(
            UpdateProfileRequest request)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out var id))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName))
            {
                return BadRequest(new
                {
                    message = "First name and last name are required."
                });
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.Phone = request.Phone?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone,
                user.Role
            });
        }
    }

    public class RegisterRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Role { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }
}
