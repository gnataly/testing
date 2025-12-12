using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TheatreCenter.DTOs;
using TheatreCenter.Services.Interfaces.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using TheatreCenter.Domain.Models;

namespace TheatreCenter.Backend.WebAPI.Controllers;

[ApiController]
[Route("/api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;

    public AuthController(IAccountService accountService, ILogger<AuthController> logger, IConfiguration configuration)
    {
        _accountService = accountService;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [SwaggerOperation(
        OperationId = "Login",
        Summary = "Аутентификация пользователя",
        Description = "Вход с именем пользователя и паролем")]
    [SwaggerResponse(200, "Успешная аутентификация", typeof(AuthResponseDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
    public async Task<IActionResult> Login([Required][FromBody] AuthRequestDto authRequestDto)
    {
        try
        {
            if (string.IsNullOrEmpty(authRequestDto.Username) || string.IsNullOrEmpty(authRequestDto.PasswordHash))
            {
                return BadRequest(new ErrorDto("ValidationError", "Username and password are required"));
            }

            var account = await _accountService.AuthenticateAsync(authRequestDto.Username, authRequestDto.PasswordHash);
            var token = GenerateJwtToken(account);

            var accountDto = new AccountDto
            {
                Id = account.Id,
                Username = account.Username,
                LastFavoritesViewDate = account.LastFavoritesViewDate,
                AccessLevel = account.AccessLevel,
                UpgradeRequest = account.UpgradeRequest
            };

            var response = new AuthResponseDto
            {
                Token = token,
                Account = accountDto
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ErrorDto("Unauthorized", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal Server Error");
            return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
        }
    }

    [HttpPost("register")]
    [SwaggerOperation(
        OperationId = "Register",
        Summary = "Регистрация нового пользователя",
        Description = "Создание нового аккаунта пользователя")]
    [SwaggerResponse(201, "Пользователь успешно создан", typeof(AuthResponseDto))]
    [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
    public async Task<IActionResult> Register([Required][FromBody] AuthRequestDto authRequestDto)
    {
        try
        {
            if (string.IsNullOrEmpty(authRequestDto.Username) || string.IsNullOrEmpty(authRequestDto.PasswordHash))
            {
                return BadRequest(new ErrorDto("ValidationError", "Username and password are required"));
            }

            var account = await _accountService.RegisterAsync(authRequestDto.Username, authRequestDto.PasswordHash);
            var token = GenerateJwtToken(account);

            var accountDto = new AccountDto
            {
                Id = account.Id,
                Username = account.Username,
                LastFavoritesViewDate = account.LastFavoritesViewDate,
                AccessLevel = account.AccessLevel,
                UpgradeRequest = account.UpgradeRequest
            };

            var response = new AuthResponseDto
            {
                Token = token,
                Account = accountDto
            };

            return CreatedAtAction(nameof(Register), response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorDto("ValidationError", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal Server Error");
            return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
        }
    }
    private string GenerateJwtToken(Account account)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                    new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                    new Claim(ClaimTypes.Name, account.Username),
                    new Claim(ClaimTypes.Role, account.AccessLevel.ToString())
                }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
