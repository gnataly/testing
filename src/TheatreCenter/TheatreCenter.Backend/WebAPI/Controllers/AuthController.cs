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
using TheatreCenter.DTOs.Auth;
using TheatreCenter.Services.Services;

namespace TheatreCenter.Backend.WebAPI.Controllers;

[ApiController]
[Route("/api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IAuthFlowService _authFlowService;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;

    public AuthController(
        IAccountService accountService,
        IAuthFlowService authFlowService,
        ILogger<AuthController> logger,
        IConfiguration configuration)
    {
        _accountService = accountService;
        _authFlowService = authFlowService;
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

            var challenge = await _authFlowService.StartTwoFactorLoginAsync(authRequestDto.Username, authRequestDto.PasswordHash);
            var account = await _accountService.GetByUsernameAsync(authRequestDto.Username)
                ?? throw new UnauthorizedAccessException("Invalid username or password");

            var accountDto = new AccountDto
            {
                Id = account.Id,
                Username = account.Username,
                LastFavoritesViewDate = account.LastFavoritesViewDate,
                AccessLevel = account.AccessLevel,
                UpgradeRequest = account.UpgradeRequest,
                LockedUntil = account.LockedUntil,
                LastPasswordChangeAt = account.LastPasswordChangedAt
            };

            return Ok(new AuthResponseDto
            {
                Token = null,
                Account = accountDto,
                RequiresTwoFactor = true,
                TwoFactorChallengeId = challenge.ChallengeId,
                TwoFactorExpiresAt = challenge.ExpiresAt,
                DeliveryChannel = challenge.DeliveryChannel,
                LockedUntil = challenge.LockedUntil
            });
        }
        catch (LockoutException ex)
        {
            return StatusCode(StatusCodes.Status423Locked, new ErrorDto("Locked", ex.Message, ex.LockedUntil.ToString("O")));
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
                UpgradeRequest = account.UpgradeRequest,
                LockedUntil = account.LockedUntil,
                LastPasswordChangeAt = account.LastPasswordChangedAt
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

    [HttpPost("verify-2fa")]
    [SwaggerOperation(
        OperationId = "VerifyTwoFactor",
        Summary = "Подтверждение двухфакторного кода",
        Description = "Проверка одноразового кода из письма и выдача JWT токена")]
    [SwaggerResponse(200, "Успешная аутентификация", typeof(AuthResponseDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    public async Task<IActionResult> VerifyTwoFactor([Required][FromBody] TwoFactorVerifyRequestDto verifyRequest)
    {
        try
        {
            var account = await _authFlowService.VerifyTwoFactorAsync(verifyRequest.Username, verifyRequest.ChallengeId, verifyRequest.Code);
            var accountDto = new AccountDto
            {
                Id = account.Id,
                Username = account.Username,
                LastFavoritesViewDate = account.LastFavoritesViewDate,
                AccessLevel = account.AccessLevel,
                UpgradeRequest = account.UpgradeRequest,
                LockedUntil = account.LockedUntil,
                LastPasswordChangeAt = account.LastPasswordChangedAt
            };

            var token = GenerateJwtToken(account);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Account = accountDto,
                RequiresTwoFactor = false,
                DeliveryChannel = "email"
            });
        }
        catch (LockoutException ex)
        {
            return StatusCode(StatusCodes.Status423Locked, new ErrorDto("Locked", ex.Message, ex.LockedUntil.ToString("O")));
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

    [HttpPost("unlock/request")]
    [SwaggerOperation(
        OperationId = "RequestUnlock",
        Summary = "Запросить код разблокировки",
        Description = "Отправляет код разблокировки на почту пользователя")]
    public async Task<IActionResult> RequestUnlock([FromBody] UnlockRequestDto unlockRequest)
    {
        try
        {
            var challenge = await _authFlowService.SendUnlockCodeAsync(unlockRequest.Username);
            return Ok(new { expiresAt = challenge.ExpiresAt });
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

    [HttpPost("unlock/verify")]
    [SwaggerOperation(
        OperationId = "VerifyUnlock",
        Summary = "Подтвердить код разблокировки",
        Description = "Проверяет код из письма и снимает блокировку с аккаунта")]
    public async Task<IActionResult> VerifyUnlock([FromBody] UnlockVerifyRequestDto verifyRequest)
    {
        try
        {
            var account = await _authFlowService.VerifyUnlockCodeAsync(verifyRequest.Username, verifyRequest.Code);
            var accountDto = new AccountDto
            {
                Id = account.Id,
                Username = account.Username,
                LastFavoritesViewDate = account.LastFavoritesViewDate,
                AccessLevel = account.AccessLevel,
                UpgradeRequest = account.UpgradeRequest,
                LockedUntil = account.LockedUntil,
                LastPasswordChangeAt = account.LastPasswordChangedAt
            };

            return Ok(accountDto);
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

    [Authorize]
    [HttpPost("change-password")]
    [SwaggerOperation(
        OperationId = "ChangePassword",
        Summary = "Смена пароля",
        Description = "Проверка текущего пароля и установка нового пароля для авторизованного пользователя")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto changePasswordRequest)
    {
        try
        {
            var currentUsername = User.Identity?.Name;
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && !string.Equals(currentUsername, changePasswordRequest.Username, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var account = await _authFlowService.ChangePasswordAsync(changePasswordRequest.Username, changePasswordRequest.CurrentPasswordHash, changePasswordRequest.NewPasswordHash);

            var accountDto = new AccountDto
            {
                Id = account.Id,
                Username = account.Username,
                LastFavoritesViewDate = account.LastFavoritesViewDate,
                AccessLevel = account.AccessLevel,
                UpgradeRequest = account.UpgradeRequest,
                LockedUntil = account.LockedUntil,
                LastPasswordChangeAt = account.LastPasswordChangedAt
            };

            return Ok(accountDto);
        }
        catch (LockoutException ex)
        {
            return StatusCode(StatusCodes.Status423Locked, new ErrorDto("Locked", ex.Message, ex.LockedUntil.ToString("O")));
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
