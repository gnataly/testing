using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TheatreCenter.DTOs;
using TheatreCenter.Services.Interfaces.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using TheatreCenter.Domain.Models;

namespace TheatreCenter.Backend.WebAPI.Controllers;

[ApiController]
[Route("/api/v1/accounts")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IAccountService accountService, ILogger<AccountsController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    [SwaggerOperation(
        OperationId = "GetAllAccounts",
        Summary = "Получить все аккаунты с фильтрацией",
        Description = "Получить пагинированный список всех аккаунтов с возможностью фильтрации по запросам на повышение (только для администраторов)")]
    [SwaggerResponse(200, "Успешное получение списка аккаунтов", typeof(AccountListDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
    public async Task<IActionResult> GetAllAccounts([FromQuery] bool? upgradeRequest, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var filter = new AccountFilter
            {
                Page = page,
                PageSize = pageSize,
                UpgradeRequest = upgradeRequest
            };

            var result = await _accountService.GetAllAsync(filter);

            var totalCount = result.Count();
            var items = result
                .OrderBy(a => a.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var accountDtos = items.Select(a => new AccountDto
            {
                Id = a.Id,
                Username = a.Username,
                LastFavoritesViewDate = a.LastFavoritesViewDate,
                AccessLevel = a.AccessLevel,
                UpgradeRequest = a.UpgradeRequest,
                LockedUntil = a.LockedUntil,
                LastPasswordChangeAt = a.LastPasswordChangedAt
            }).ToList();

            var response = new AccountListDto
            {
                Items = accountDtos,
                Pagination = new PaginationDto
                {
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal Server Error");
            return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
        }
    }

    [HttpGet("{accountId}")]
    [Authorize(Policy = "AdminOrUser")]
    [SwaggerOperation(
        OperationId = "GetAccountByAccountId",
        Summary = "Получить аккаунт по ID",
        Description = "Получить информацию об аккаунте по его ID (пользователи могут получать только свой аккаунт)")]
    [SwaggerResponse(200, "Успешное получение аккаунта", typeof(AccountDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> GetAccountByAccountId([Required][FromRoute] int accountId)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var isAdmin = User.IsInRole("Admin");

            // Users can only access their own account unless they are admin
            if (currentUserId != accountId && !isAdmin)
            {
                return Forbid();
            }

            var account = await _accountService.GetByIdAsync(accountId);
            if (account == null)
            {
                return NotFound(new ErrorDto("NotFound", "Account not found"));
            }

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal Server Error");
            return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
        }
    }

    [HttpPut("{accountId}")]
    [Authorize(Policy = "AdminOrUser")]
    [SwaggerOperation(
        OperationId = "UpdateAccountByAccountId",
        Summary = "Обновить аккаунт",
        Description = "Обновить информацию аккаунта (пользователи могут обновлять только свой аккаунт)")]
    [SwaggerResponse(200, "Аккаунт успешно обновлен", typeof(AccountDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
    [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
    public async Task<IActionResult> UpdateAccountByAccountId([Required][FromRoute] int accountId, [Required][FromBody] UpdateAccountRequestDto updateAccountRequestDto)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var isAdmin = User.IsInRole("Admin");

            // Users can only update their own account unless they are admin
            if (currentUserId != accountId && !isAdmin)
            {
                return Forbid();
            }

            var account = await _accountService.GetByIdAsync(accountId);
            if (account == null)
            {
                return NotFound(new ErrorDto("NotFound", "Account not found"));
            }

            account.Username = updateAccountRequestDto.Username;
            account.ChangePassword(updateAccountRequestDto.PasswordHash);

            await _accountService.UpdateAsync(account);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal Server Error");
            return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
        }
    }

    [HttpDelete("{accountId}")]
    [Authorize(Policy = "AdminOrUser")]
    [SwaggerOperation(
        OperationId = "DeleteAccountByAccountId",
        Summary = "Удалить аккаунт",
        Description = "Удалить аккаунт (пользователи могут удалять только свой аккаунт)")]
    [SwaggerResponse(204, "Аккаунт успешно удален")]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> DeleteAccountByAccountId([Required][FromRoute] int accountId)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var isAdmin = User.IsInRole("Admin");

            // Users can only delete their own account unless they are admin
            if (currentUserId != accountId && !isAdmin)
            {
                return Forbid();
            }

            var account = await _accountService.GetByIdAsync(accountId);
            if (account == null)
            {
                return NotFound(new ErrorDto("NotFound", "Account not found"));
            }

            await _accountService.DeleteAsync(accountId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal Server Error");
            return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
        }
    }

    [HttpPost("{accountId}/upgrade_request")]
    [Authorize(Policy = "AdminOrUser")]
    [SwaggerOperation(
        OperationId = "CreateUpgradeRequestByAccountId",
        Summary = "Запросить повышение прав доступа",
        Description = "Отправить запрос на повышение прав доступа до администратора")]
    [SwaggerResponse(200, "Запрос на повышение успешно отправлен", typeof(AccountDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> CreateUpgradeRequestByAccountId([Required][FromRoute] int accountId)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (currentUserId != accountId)
            {
                return Forbid();
            }

            var result = await _accountService.SubmitUpgradeRequestAsync(accountId);
            if (!result)
            {
                return NotFound(new ErrorDto("NotFound", "Account not found"));
            }

            var account = await _accountService.GetByIdAsync(accountId);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal Server Error");
            return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
        }
    }

    [HttpPost("{accountId}/upgrade_approval")]
    [Authorize(Policy = "AdminOnly")]
    [SwaggerOperation(
        OperationId = "UpgradeApprovalByAccountId",
        Summary = "Одобрить/отклонить запрос на повышение прав",
        Description = "Одобрить или отклонить запрос на повышение прав доступа (только для администраторов)")]
    [SwaggerResponse(200, "Запрос на повышение успешно обработан", typeof(AccountDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> UpgradeApprovalByAccountId([Required][FromRoute] int accountId, [Required][FromBody] UpgradeRequestDto upgradeRequestDto)
    {
        try
        {
            var result = await _accountService.ProcessUpgradeRequestAsync(accountId, upgradeRequestDto.IsApproved);
            if (!result)
            {
                return NotFound(new ErrorDto("NotFound", "Account not found"));
            }

            var account = await _accountService.GetByIdAsync(accountId);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal Server Error");
            return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
        }
    }
}
