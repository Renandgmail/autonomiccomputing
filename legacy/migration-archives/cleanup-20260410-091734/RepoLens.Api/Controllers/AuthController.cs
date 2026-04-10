using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RepoLens.Core.Entities;
using RepoLens.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace RepoLens.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestModel request)
    {
        var requestId = Guid.NewGuid().ToString()[..8];
        _logger.LogInformation("[{RequestId}] Registration attempt started for email: {Email}", 
            requestId, request.Email);

        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("[{RequestId}] Registration failed: Missing email or password", requestId);
                return BadRequest(new AuthResponseModel 
                { 
                    Success = false, 
                    ErrorMessage = "Email and password are required",
                    DebugInfo = $"Email: {!string.IsNullOrWhiteSpace(request.Email)}, Password: {!string.IsNullOrWhiteSpace(request.Password)}"
                });
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("[{RequestId}] Registration failed: User already exists with email: {Email}", 
                    requestId, request.Email);
                return BadRequest(new AuthResponseModel 
                { 
                    Success = false, 
                    ErrorMessage = "User already exists with this email",
                    DebugInfo = $"Existing user ID: {existingUser.Id}, Active: {existingUser.IsActive}"
                });
            }

            _logger.LogInformation("[{RequestId}] Creating new user for email: {Email}", requestId, request.Email);

            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Preferences = new UserPreferences()
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("[{RequestId}] User creation failed for {Email}. Errors: {Errors}", 
                    requestId, request.Email, errors);
                
                return BadRequest(new AuthResponseModel 
                { 
                    Success = false, 
                    ErrorMessage = errors,
                    DebugInfo = $"Identity errors: {string.Join(" | ", result.Errors.Select(e => $"{e.Code}: {e.Description}"))}"
                });
            }

            _logger.LogInformation("[{RequestId}] User created successfully. ID: {UserId}, Email: {Email}", 
                requestId, user.Id, user.Email);

            var token = GenerateJwtToken(user);
            _logger.LogInformation("[{RequestId}] JWT token generated for user: {UserId}", requestId, user.Id);
            
            var response = new AuthResponseModel 
            { 
                Success = true, 
                Token = token,
                User = new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive
                },
                DebugInfo = $"User registered successfully. ID: {user.Id}, Token generated: {!string.IsNullOrEmpty(token)}"
            };

            _logger.LogInformation("[{RequestId}] Registration completed successfully for user: {UserId}", 
                requestId, user.Id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{RequestId}] Registration failed with exception for email: {Email}. Exception: {ExceptionType}: {Message}", 
                requestId, request.Email, ex.GetType().Name, ex.Message);
            
            return StatusCode(500, new AuthResponseModel 
            { 
                Success = false, 
                ErrorMessage = "Registration failed due to server error",
                DebugInfo = $"Exception: {ex.GetType().Name}: {ex.Message}. Stack trace: {ex.StackTrace}"
            });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
    {
        var requestId = Guid.NewGuid().ToString()[..8];
        _logger.LogInformation("[{RequestId}] Login attempt started for email: {Email}", 
            requestId, request.Email);

        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("[{RequestId}] Login failed: Missing email or password", requestId);
                return BadRequest(new AuthResponseModel 
                { 
                    Success = false, 
                    ErrorMessage = "Email and password are required",
                    DebugInfo = $"Email provided: {!string.IsNullOrWhiteSpace(request.Email)}, Password provided: {!string.IsNullOrWhiteSpace(request.Password)}"
                });
            }

            _logger.LogInformation("[{RequestId}] Looking up user by email: {Email}", requestId, request.Email);
            
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("[{RequestId}] Login failed: User not found for email: {Email}", requestId, request.Email);
                return BadRequest(new AuthResponseModel 
                { 
                    Success = false, 
                    ErrorMessage = "Invalid credentials",
                    DebugInfo = "User not found in database"
                });
            }

            _logger.LogInformation("[{RequestId}] User found. ID: {UserId}, Active: {IsActive}, Email Confirmed: {EmailConfirmed}", 
                requestId, user.Id, user.IsActive, user.EmailConfirmed);

            if (!user.IsActive)
            {
                _logger.LogWarning("[{RequestId}] Login failed: User account is inactive. UserID: {UserId}", 
                    requestId, user.Id);
                return BadRequest(new AuthResponseModel 
                { 
                    Success = false, 
                    ErrorMessage = "Account is inactive",
                    DebugInfo = $"User account {user.Id} is marked as inactive"
                });
            }

            _logger.LogInformation("[{RequestId}] Checking password for user: {UserId}", requestId, user.Id);
            
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            
            _logger.LogInformation("[{RequestId}] Password check result: Succeeded={Succeeded}, IsLockedOut={IsLockedOut}, IsNotAllowed={IsNotAllowed}, RequiresTwoFactor={RequiresTwoFactor}", 
                requestId, result.Succeeded, result.IsLockedOut, result.IsNotAllowed, result.RequiresTwoFactor);
            
            if (!result.Succeeded)
            {
                string debugInfo;
                if (result.IsLockedOut)
                    debugInfo = "Account is locked out";
                else if (result.IsNotAllowed)
                    debugInfo = "Sign in is not allowed for this user";
                else if (result.RequiresTwoFactor)
                    debugInfo = "Two-factor authentication is required";
                else
                    debugInfo = "Invalid password";

                _logger.LogWarning("[{RequestId}] Login failed for user {UserId}: {DebugInfo}", 
                    requestId, user.Id, debugInfo);
                
                return BadRequest(new AuthResponseModel 
                { 
                    Success = false, 
                    ErrorMessage = "Invalid credentials",
                    DebugInfo = debugInfo
                });
            }

            _logger.LogInformation("[{RequestId}] Password verification successful for user: {UserId}", requestId, user.Id);

            var token = GenerateJwtToken(user);
            _logger.LogInformation("[{RequestId}] JWT token generated for user: {UserId}, Token length: {TokenLength}", 
                requestId, user.Id, token?.Length ?? 0);
            
            var response = new AuthResponseModel 
            { 
                Success = true, 
                Token = token,
                User = new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive
                },
                DebugInfo = $"Login successful. UserID: {user.Id}, Token generated: {!string.IsNullOrEmpty(token)}, Token expires in 60 minutes"
            };

            _logger.LogInformation("[{RequestId}] Login completed successfully for user: {UserId}", 
                requestId, user.Id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{RequestId}] Login failed with exception for email: {Email}. Exception: {ExceptionType}: {Message}", 
                requestId, request.Email, ex.GetType().Name, ex.Message);
            
            return StatusCode(500, new AuthResponseModel 
            { 
                Success = false, 
                ErrorMessage = "Login failed due to server error",
                DebugInfo = $"Exception: {ex.GetType().Name}: {ex.Message}. Stack trace: {ex.StackTrace}"
            });
        }
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var secretKey = _configuration["JwtSettings:SecretKey"] ?? "ThisIsASecure256BitKeyForJWTTokenGenerationThatMustBeLongEnough!@#$%^&*()";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var issuer = _configuration["JwtSettings:Issuer"] ?? "RepoLens.Api";
        var audience = _configuration["JwtSettings:Audience"] ?? "RepoLens.Web";
        var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(expirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
