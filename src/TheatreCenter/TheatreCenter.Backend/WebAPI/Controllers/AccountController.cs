using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Models;
using TheatreCenter.DTOs.Account;
using TheatreCenter.DTOs.Favorite;
using TheatreCenter.Services.Interfaces.Services;

namespace TheatreCenter.Backend.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IConfiguration _configuration;

        public AccountsController(IAccountService accountService, IConfiguration configuration)
        {
            _accountService = accountService;
            _configuration = configuration;
        }

        [HttpPost("auth/login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AuthDTO authDto)
        {
            try
            {
                var account = await _accountService.AuthenticateAsync(authDto.Username, authDto.PasswordHash);

                if (account == null)
                    return Unauthorized("Invalid username or password");

                var token = GenerateJwtToken(account);

                return Ok(new AuthResponse
                {
                    Token = token,
                    Account = new AccountDTO(
                        account.Id,
                        account.Username,
                        account.LastFavoritesViewDate,
                        account.AccessLevel)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("auth/register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] AuthDTO authDto)
        {
            try
            {
                var account = await _accountService.RegisterAsync(
                    authDto.Username,
                    authDto.PasswordHash,
                    AccessLevel.User);

                var token = GenerateJwtToken(account);

                return Ok(new AuthResponse
                {
                    Token = token,
                    Account = new AccountDTO(
                        account.Id,
                        account.Username,
                        account.LastFavoritesViewDate,
                        account.AccessLevel)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var account = await _accountService.GetByIdAsync(id);
                if (account == null)
                    return NotFound();

                return Ok(new AccountDTO(
                    account.Id,
                    account.Username,
                    account.LastFavoritesViewDate,
                    account.AccessLevel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var accounts = await _accountService.GetAllAsync();
                var accountDtos = accounts.Select(a => new AccountDTO(
                    a.Id,
                    a.Username,
                    a.LastFavoritesViewDate,
                    a.AccessLevel));

                return Ok(accountDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrUser")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAccountDTO updateDto)
        {
            try
            {
                // Проверка, что пользователь обновляет только свой аккаунт (если он не админ)
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (currentUserId != id && !User.IsInRole(AccessLevel.Admin.ToString()))
                {
                    return Forbid();
                }

                var account = await _accountService.GetByIdAsync(id);
                if (account == null)
                    return NotFound();

                account.Username = updateDto.Username;

                await _accountService.UpdateAsync(account);

                return Ok(new AccountDTO(
                    account.Id,
                    account.Username,
                    account.LastFavoritesViewDate,
                    account.AccessLevel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrUser")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _accountService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{accountId}/favorites/actors/{actorId}")]
        [Authorize(Policy = "AdminOrUser")]
        public async Task<IActionResult> AddFavoriteActor([FromBody] AddFavoriteDTO addFavoriteDTO)
        {
            try
            {
                int accountId = addFavoriteDTO.AccountId;
                int actorId = addFavoriteDTO.TargetId;
                // Проверка, что пользователь изменяет только свои избранные
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (currentUserId != accountId && !User.IsInRole(AccessLevel.Admin.ToString()))
                {
                    return Forbid();
                }

                var result = await _accountService.AddFavoriteActorAsync(accountId, actorId);
                return result ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{accountId}/favorites/actors/{actorId}")]
        [Authorize(Policy = "AdminOrUser")]
        public async Task<IActionResult> RemoveFavoriteActor([FromBody] RemoveFavoriteDTO removeFavoriteDTO)
        {
            int accountId = removeFavoriteDTO.AccountId;
            int actorId = removeFavoriteDTO.TargetId;
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (currentUserId != accountId && !User.IsInRole(AccessLevel.Admin.ToString()))
                {
                    return Forbid();
                }

                var result = await _accountService.RemoveFavoriteActorAsync(accountId, actorId);
                return result ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{accountId}/favorites/musicals/{musicalId}")]
        [Authorize(Policy = "AdminOrUser")]
        public async Task<IActionResult> AddFavoriteMusical([FromBody] AddFavoriteDTO addFavoriteDTO)
        {
            try
            {
                int accountId = addFavoriteDTO.AccountId;
                int musicalId = addFavoriteDTO.TargetId;
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (currentUserId != accountId && !User.IsInRole(AccessLevel.Admin.ToString()))
                {
                    return Forbid();
                }

                var result = await _accountService.AddFavoriteMusicalAsync(accountId, musicalId);
                return result ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{accountId}/favorites/musicals/{musicalId}")]
        [Authorize(Policy = "AdminOrUser")]
        public async Task<IActionResult> RemoveFavoriteMusical([FromBody] RemoveFavoriteDTO removeFavoriteDTO)
        {
            try
            {
                int accountId = removeFavoriteDTO.AccountId;
                int musicalId = removeFavoriteDTO.TargetId;
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (currentUserId != accountId && !User.IsInRole(AccessLevel.Admin.ToString()))
                {
                    return Forbid();
                }

                var result = await _accountService.RemoveFavoriteMusicalAsync(accountId, musicalId);
                return result ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{accountId}/favorites/theatres/{theatreId}")]
        [Authorize(Policy = "AdminOrUser")]
        public async Task<IActionResult> AddFavoriteTheatre([FromBody] AddFavoriteDTO addFavoriteDTO)
        {
            try
            {
                int accountId = addFavoriteDTO.AccountId;
                int theatreId = addFavoriteDTO.TargetId;
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (currentUserId != accountId && !User.IsInRole(AccessLevel.Admin.ToString()))
                {
                    return Forbid();
                }

                var result = await _accountService.AddFavoriteTheatreAsync(accountId, theatreId);
                return result ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{accountId}/favorites/theatres/{theatreId}")]
        [Authorize(Policy = "AdminOrUser")]
        public async Task<IActionResult> RemoveFavoriteTheatre([FromBody] RemoveFavoriteDTO removeFavoriteDTO)
        {
            try
            {
                int accountId = removeFavoriteDTO.AccountId;
                int theatreId = removeFavoriteDTO.TargetId;
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (currentUserId != accountId && !User.IsInRole(AccessLevel.Admin.ToString()))
                {
                    return Forbid();
                }

                var result = await _accountService.RemoveFavoriteTheatreAsync(accountId, theatreId);
                return result ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{accountId}/favorites")]
        [Authorize(Policy = "AdminOrUser")]
        public async Task<IActionResult> GetFavorites(int accountId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (currentUserId != accountId && !User.IsInRole(AccessLevel.Admin.ToString()))
                {
                    return Forbid();
                }

                var favorites = await _accountService.GetFavoritesAsync(accountId);
                return Ok(favorites);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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


        //[HttpPost("{id}/upgrade-request")]
        //[Authorize(Policy = "AdminOrUser")]
        //public async Task<IActionResult> SubmitUpgradeRequest(int id)
        //{
        //    try
        //    {
        //        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //        if (currentUserId != id)
        //        {
        //            return Forbid();
        //        }

        //        var result = await _accountService.SubmitUpgradeRequestAsync(id);
        //        return result ? Ok() : NotFound();
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[HttpGet("upgrade-requests")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> GetUpgradeRequests()
        //{
        //    try
        //    {
        //        var accounts = await _accountService.GetAccountsWithUpgradeRequestAsync();
        //        var accountDtos = accounts.Select(a => new AccountDTO(
        //            a.Id,
        //            a.Username,
        //            a.LastFavoritesViewDate,
        //            a.AccessLevel,
        //            a.UpgradeRequest));

        //        return Ok(accountDtos);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[HttpPost("{id}/process-upgrade")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> ProcessUpgradeRequest(int id, [FromBody] bool isApproved)
        //{
        //    try
        //    {
        //        var result = await _accountService.ProcessUpgradeRequestAsync(id, isApproved);
        //        return result ? Ok() : NotFound();
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
    }
}